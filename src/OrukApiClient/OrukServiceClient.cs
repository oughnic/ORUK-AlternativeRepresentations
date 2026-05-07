using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrukApiClient.Internal;
using OrukModels.Models;

namespace OrukApiClient;

/// <summary>
/// Queries an ORUK v3 /services endpoint with pagination, optional server-side
/// filtering, and client-side fallback filtering for criteria the endpoint may
/// not support natively.
/// </summary>
public sealed class OrukServiceClient : IOrukServiceClient
{
    private const int MaxPageSize = 100;

    private readonly HttpClient _httpClient;
    private readonly ILogger<OrukServiceClient> _logger;

    public OrukServiceClient(HttpClient httpClient, ILogger<OrukServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<OrukService> SearchAsync(
        Uri feedBaseUrl,
        OrukServiceQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var noLimit = query.MaxRecords < 1;
        var remaining = noLimit ? int.MaxValue : query.MaxRecords;
        // Always request the maximum page size regardless of MaxRecords.
        // Many ORUK endpoints do not filter by the `text` parameter, so client-side
        // keyword filtering may discard most records. Fetching in large batches
        // minimises round-trips when filtering is heavy.
        var pageSize = MaxPageSize;

        int totalPages = 1;
        int currentPage = 1;
        bool firstPage = true;

        while (currentPage <= totalPages && remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var url = OrukUrlBuilder.BuildServicesUrl(feedBaseUrl, query, currentPage, pageSize);

            OrukPage<OrukService>? page = null;
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    LogHttpError(response, currentPage, url);
                    if (firstPage) { yield break; }
                    currentPage++;
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                page = TryDeserialise(body, currentPage, url);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                if (firstPage)
                {
                    _logger.LogError(ex, "Fatal error fetching first page from {Url}. Aborting.", url);
                    yield break;
                }
                _logger.LogWarning(ex, "Error fetching page {Page} from {Url}. Skipping.", currentPage, url);
                currentPage++;
                continue;
            }

            if (page is null || page.Contents.Count == 0)
            {
                _logger.LogDebug("Page {Page} returned no contents. Stopping pagination.", currentPage);
                yield break;
            }

            if (firstPage)
            {
                totalPages = Math.Max(page.TotalPages, 1);
                firstPage = false;
                _logger.LogInformation(
                    "Feed reports {Total} services across {Pages} pages at {BaseUrl}.",
                    page.TotalItems, page.TotalPages, OrukUrlBuilder.EnsureBase(feedBaseUrl));
            }

            foreach (var service in page.Contents)
            {
                if (remaining <= 0) yield break;

                if (!MatchesClientSideFilters(service, query))
                    continue;

                yield return service;
                remaining--;
            }

            currentPage++;
        }
    }

    /// <inheritdoc/>
    public async Task<OrukService?> GetByIdAsync(
        Uri feedBaseUrl,
        string serviceId,
        CancellationToken cancellationToken = default)
    {
        var url = OrukUrlBuilder.BuildServiceByIdUrl(feedBaseUrl, serviceId);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Service {Id} not found at {Url}.", serviceId, url);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            // Single-service endpoints return the object directly, not wrapped in a page.
            return JsonSerializer.Deserialize<OrukService>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            _logger.LogError(ex, "Failed to retrieve service {Id} from {Url}.", serviceId, url);
            return null;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private OrukPage<OrukService>? TryDeserialise(string body, int page, Uri url)
    {
        try
        {
            return JsonSerializer.Deserialize<OrukPage<OrukService>>(body);
        }
        catch (JsonException)
        {
            _logger.LogWarning("Deserialisation failed for page {Page} from {Url}. Retrying case-insensitive.", page, url);
        }

        try
        {
            return JsonSerializer.Deserialize<OrukPage<OrukService>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Fallback deserialisation also failed for page {Page} from {Url}.", page, url);
            return null;
        }
    }

    private void LogHttpError(HttpResponseMessage response, int page, Uri url)
    {
        var code = (int)response.StatusCode;
        var reason = response.ReasonPhrase ?? "Unknown";
        _logger.LogError(
            "HTTP {Code} ({Reason}) fetching page {Page} from {Url}.",
            code, reason, page, url);
    }

    /// <summary>
    /// Applies filter criteria that may not be supported as server-side query
    /// parameters by all ORUK endpoint implementations.
    /// </summary>
    private static bool MatchesClientSideFilters(OrukService service, OrukServiceQuery query)
    {
        // Client-side keyword fallback — applied when no taxonomy terms were resolved.
        // Many ORUK endpoints do not implement the `text` query parameter; this ensures
        // the keyword still filters results even if the server returned unfiltered data.
        if (!string.IsNullOrWhiteSpace(query.Keyword) && query.TaxonomyTermIds.Count == 0)
        {
            var kw = query.Keyword;
            var inName = service.Name?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false;
            var inDesc = service.Description?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false;
            var inOrg  = service.Organization?.Name?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false;
            if (!inName && !inDesc && !inOrg)
                return false;
        }

        // Age range
        if (query.MinimumAge.HasValue &&
            service.MaximumAge.HasValue &&
            service.MaximumAge.Value < query.MinimumAge.Value)
            return false;

        if (query.MaximumAge.HasValue &&
            service.MinimumAge.HasValue &&
            service.MinimumAge.Value > query.MaximumAge.Value)
            return false;

        // Free only
        if (query.FreeOnly)
        {
            var hasCost = service.CostOptions.Any(c => c.Amount.HasValue && c.Amount.Value > 0m);
            if (hasCost) return false;
        }

        // Taxonomy term filter (client-side fallback)
        if (query.TaxonomyTermIds.Count > 0)
        {
            var serviceTermIds = service.Attributes
                .Where(a => a.TaxonomyTerm is not null)
                .Select(a => a.TaxonomyTerm!.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!query.TaxonomyTermIds.Any(id => serviceTermIds.Contains(id)))
                return false;
        }

        // Language filter
        if (query.Language is not null)
        {
            var lang = query.Language;
            var hasLanguage = service.Languages.Any(l =>
                (l.Name?.Contains(lang, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.Code?.Equals(lang, StringComparison.OrdinalIgnoreCase) ?? false));
            if (!hasLanguage) return false;
        }

        // Accessibility feature filter
        if (query.AccessibilityFeature is not null)
        {
            var feature = query.AccessibilityFeature;
            var hasFeature = service.ServiceAtLocations
                .Select(sal => sal.Location)
                .Where(l => l is not null)
                .SelectMany(l => l!.Accessibility)
                .Any(a => a.Description?.Contains(feature, StringComparison.OrdinalIgnoreCase) ?? false);
            if (!hasFeature) return false;
        }

        // Delivery type filter (matches OrukLocation.LocationType)
        if (query.DeliveryType is not null)
        {
            var type = query.DeliveryType;
            var hasType = service.ServiceAtLocations
                .Select(sal => sal.Location)
                .Any(l => l?.LocationType?.Equals(type, StringComparison.OrdinalIgnoreCase) ?? false);
            if (!hasType) return false;
        }

        // Updated since filter
        if (query.UpdatedSince.HasValue)
        {
            if (string.IsNullOrWhiteSpace(service.LastModified))
                return false;

            if (!DateTimeOffset.TryParse(service.LastModified, out var modified) ||
                modified < query.UpdatedSince.Value)
                return false;
        }

        return true;
    }
}
