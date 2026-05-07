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
/// <remarks>
/// Supports both standard page-number pagination and RPDE (Realtime Paged Data
/// Exchange) feeds that return a <c>next_url</c> cursor link.  The mode is
/// detected automatically from the first response.
/// UK postcodes passed as proximity are geocoded via postcodes.io before being
/// forwarded to the ORUK endpoint, which requires lat,long coordinates.
/// </remarks>
public sealed class OrukServiceClient : IOrukServiceClient
{
    private const int MaxPageSize = 100;

    private readonly HttpClient _httpClient;
    private readonly ILogger<OrukServiceClient> _logger;
    private readonly IPostcodeGeocoder _geocoder;

    public OrukServiceClient(
        HttpClient httpClient,
        ILogger<OrukServiceClient> logger,
        IPostcodeGeocoder geocoder)
    {
        _httpClient = httpClient;
        _logger = logger;
        _geocoder = geocoder;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<OrukService> SearchAsync(
        Uri feedBaseUrl,
        OrukServiceQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var noLimit = query.MaxRecords < 1;
        var remaining = noLimit ? int.MaxValue : query.MaxRecords;
        var pageSize = MaxPageSize;

        // Geocode the proximity string to lat,long — ORUK endpoints require coordinates,
        // not postcodes or place names. Passing a raw postcode returns zero results.
        string? resolvedProximity = null;
        if (!string.IsNullOrWhiteSpace(query.Proximity))
        {
            var geoPoint = await _geocoder.LookupAsync(query.Proximity, cancellationToken);
            if (geoPoint is not null)
                resolvedProximity = geoPoint.ToString();
            else
                _logger.LogWarning(
                    "Could not geocode '{Location}' to coordinates. " +
                    "The proximity filter will be omitted from the API request — " +
                    "results will not be geographically filtered.",
                    query.Proximity);
        }

        // RPDE mode: when a feed returns next_url we follow cursor links instead
        // of incrementing page numbers.
        Uri? rpdeNextUrl = null;
        int totalPages = 1;
        int currentPage = 1;
        bool firstPage = true;

        while (remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Uri url;
            if (rpdeNextUrl is not null)
            {
                url = rpdeNextUrl;
            }
            else
            {
                if (currentPage > totalPages) break;
                url = OrukUrlBuilder.BuildServicesUrl(
                    feedBaseUrl, query, currentPage, pageSize, resolvedProximity);
            }

            OrukPage<OrukService>? page = null;
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    LogHttpError(response, currentPage, url);
                    if (firstPage) yield break;
                    if (rpdeNextUrl is not null) break;
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
                if (rpdeNextUrl is not null) break;
                currentPage++;
                continue;
            }

            if (page is null || page.Contents.Count == 0)
            {
                // RPDE feeds may return an empty page with a next_url — stop only
                // when the next_url is also absent (end of feed).
                if (page?.NextUrl is null)
                {
                    _logger.LogDebug("Page {Page} returned no contents. Stopping pagination.", currentPage);
                    yield break;
                }
            }

            if (firstPage)
            {
                firstPage = false;

                if (page!.NextUrl is not null)
                {
                    _logger.LogInformation(
                        "Feed at {BaseUrl} uses RPDE pagination (next_url present).",
                        OrukUrlBuilder.EnsureBase(feedBaseUrl));
                }
                else
                {
                    totalPages = Math.Max(page.TotalPages, 1);
                    _logger.LogInformation(
                        "Feed reports {Total} services across {Pages} pages at {BaseUrl}.",
                        page.TotalItems, page.TotalPages, OrukUrlBuilder.EnsureBase(feedBaseUrl));
                }
            }

            foreach (var service in page!.Contents)
            {
                if (remaining <= 0) yield break;

                if (!MatchesClientSideFilters(service, query))
                    continue;

                yield return service;
                remaining--;
            }

            // Advance to next page
            if (page.NextUrl is not null)
            {
                if (!Uri.TryCreate(page.NextUrl, UriKind.Absolute, out var nextUri))
                {
                    _logger.LogWarning(
                        "RPDE next_url '{NextUrl}' is not a valid absolute URI. Stopping.", page.NextUrl);
                    yield break;
                }
                rpdeNextUrl = nextUri;
            }
            else
            {
                rpdeNextUrl = null;
                if (currentPage >= totalPages) break;
                currentPage++;
            }
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

        // Delivery type filter
        // Primary: match OrukLocation.LocationType (e.g. "physical", "virtual", "postal")
        // Fallback: keyword match on description/name when LocationType is not populated,
        // which is common in many ORUK feed implementations.
        if (query.DeliveryType is not null)
        {
            var type = query.DeliveryType;

            var hasLocationType = service.ServiceAtLocations
                .Select(sal => sal.Location)
                .Any(l => l?.LocationType?.Equals(type, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!hasLocationType)
                hasLocationType = MatchesDeliveryTypeFallback(service, type);

            if (!hasLocationType) return false;
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

    /// <summary>
    /// Fallback delivery-type check for when <c>location_type</c> is not populated.
    /// Uses description/name keywords and structural signals (no physical address + has URL).
    /// </summary>
    private static bool MatchesDeliveryTypeFallback(OrukService service, string deliveryType)
    {
        var isVirtual = deliveryType.Equals("virtual", StringComparison.OrdinalIgnoreCase)
                     || deliveryType.Equals("online", StringComparison.OrdinalIgnoreCase)
                     || deliveryType.Equals("remote", StringComparison.OrdinalIgnoreCase);

        if (!isVirtual) return false;

        // Signal 1: service description or name mentions online delivery
        var text = $"{service.Name} {service.Description}";
        string[] virtualKeywords = ["online", "virtual", "remote", "zoom", "teams", "webinar", "digital", "telephone", "phone"];
        if (virtualKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Signal 2: service has a URL but no service-at-location with a physical address
        if (service.Url is not null && service.ServiceAtLocations.Count == 0)
            return true;

        return false;
    }
}
