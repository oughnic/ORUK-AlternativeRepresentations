using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrukApiClient;
using OrukModels.Models;

namespace OrukTransformer.Mcp.Taxonomy;

/// <summary>
/// Thread-safe in-memory cache for ORUK taxonomy terms.
/// Each feed's terms are cached independently with a configurable TTL.
/// </summary>
internal sealed class TaxonomyCache : ITaxonomyCache
{
    private readonly IOrukTaxonomyClient _taxonomyClient;
    private readonly McpOptions _options;
    private readonly ILogger<TaxonomyCache> _logger;

    private readonly Dictionary<string, CacheEntry> _cache = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TaxonomyCache(
        IOrukTaxonomyClient taxonomyClient,
        IOptions<McpOptions> options,
        ILogger<TaxonomyCache> logger)
    {
        _taxonomyClient = taxonomyClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OrukTaxonomyTerm>> GetTermsAsync(
        Uri feedBaseUrl,
        CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(feedBaseUrl);

        // Fast path: no lock needed for a valid cached entry.
        if (_cache.TryGetValue(key, out var cached) && !cached.IsExpired)
        {
            _logger.LogDebug("Taxonomy cache hit for {FeedUrl}.", key);
            return cached.Terms;
        }

        // Slow path: acquire the lock and double-check.
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(key, out cached) && !cached.IsExpired)
            {
                _logger.LogDebug("Taxonomy cache hit for {FeedUrl} (post-lock).", key);
                return cached.Terms;
            }
        }
        finally
        {
            // Release the lock before the network call so other feeds are not blocked.
            _lock.Release();
        }

        // Fetch outside the lock — concurrent requests to different feeds proceed freely.
        _logger.LogInformation("Loading taxonomy terms for {FeedUrl}.", key);
        var terms = await _taxonomyClient.GetAllTermsAsync(feedBaseUrl, cancellationToken);

        // Re-acquire to store the result.
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var ttl = TimeSpan.FromMinutes(_options.TaxonomyCacheTtlMinutes);
            _cache[key] = new CacheEntry(terms, DateTimeOffset.UtcNow.Add(ttl));
        }
        finally
        {
            _lock.Release();
        }

        return terms;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> ResolveAsync(
        string label,
        Uri feedBaseUrl,
        CancellationToken cancellationToken = default)
    {
        var terms = await GetTermsAsync(feedBaseUrl, cancellationToken);
        return _taxonomyClient.ResolveByLabel(label, terms);
    }

    /// <inheritdoc/>
    public void InvalidateAll()
    {
        _lock.Wait();
        try { _cache.Clear(); }
        finally { _lock.Release(); }
    }

    /// <summary>
    /// Normalises a feed base URL into a stable cache key — strips trailing slash
    /// and any trailing <c>/services</c> segment, then lower-cases.
    /// </summary>
    private static string NormalizeKey(Uri feedBaseUrl)
    {
        var s = feedBaseUrl.ToString().TrimEnd('/');
        if (s.EndsWith("/services", StringComparison.OrdinalIgnoreCase))
            s = s[..^"/services".Length];
        return s.ToLowerInvariant();
    }

    private sealed record CacheEntry(
        IReadOnlyList<OrukTaxonomyTerm> Terms,
        DateTimeOffset ExpiresAt)
    {
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    }
}
