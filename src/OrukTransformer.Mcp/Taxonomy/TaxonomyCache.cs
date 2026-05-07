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
        var key = feedBaseUrl.ToString();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
            {
                _logger.LogDebug("Taxonomy cache hit for {FeedUrl}.", key);
                return entry.Terms;
            }

            _logger.LogInformation("Loading taxonomy terms for {FeedUrl}.", key);
            var terms = await _taxonomyClient.GetAllTermsAsync(feedBaseUrl, cancellationToken);
            var ttl = TimeSpan.FromMinutes(_options.TaxonomyCacheTtlMinutes);
            _cache[key] = new CacheEntry(terms, DateTimeOffset.UtcNow.Add(ttl));
            return terms;
        }
        finally
        {
            _lock.Release();
        }
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

    private sealed record CacheEntry(
        IReadOnlyList<OrukTaxonomyTerm> Terms,
        DateTimeOffset ExpiresAt)
    {
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    }
}
