using Microsoft.Extensions.Logging;
using OrukModels.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OrukTransformer.Cli.Fetching;

/// <summary>
/// Fetches ORUK service records by iterating the paged <c>GET /services</c>
/// endpoint.
/// </summary>
/// <remarks>
/// <para>
/// Two paging strategies are supported and detected automatically from the
/// first page response:
/// </para>
/// <list type="bullet">
/// <item><description>
///   <strong>RDPE (Realtime Paged Data Exchange)</strong> – when the first
///   response contains a non-empty <c>next_url</c> or <c>nextUrl</c> field,
///   subsequent pages are retrieved by following those URLs directly.
///   Pagination ends when the next-page URL is absent or empty.
/// </description></item>
/// <item><description>
///   <strong>Standard ORUK page-number paging</strong> – when no next-page
///   URL is present, query parameters <c>page</c> and <c>per_page</c> are
///   used to iterate pages.  Page size is capped at 100 records per request.
/// </description></item>
/// </list>
/// <para>
/// If an individual page fails to deserialise, a warning is logged and that
/// page is skipped ("receive liberally").  If the <em>first</em> page returns
/// a non-success HTTP status code the fetch is aborted immediately.
/// </para>
/// </remarks>
public sealed class OrukFeedPageFetcher : IOrukFeedPageFetcher
{
    private const int MaxPageSize = 100;

    private readonly HttpClient _httpClient;
    private readonly ILogger<OrukFeedPageFetcher> _logger;

    public OrukFeedPageFetcher(HttpClient httpClient, ILogger<OrukFeedPageFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<OrukService> FetchAsync(
        Uri endpointUrl,
        int maxRecords,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var noLimit = maxRecords < 1;
        var remaining = noLimit ? int.MaxValue : maxRecords;
        var pageSize = noLimit ? MaxPageSize : Math.Min(maxRecords, MaxPageSize);

        // State for standard page-number paging
        int totalPages = 1;
        int currentPage = 1;

        // State for RDPE paging (set after first page is received)
        bool useRdpe = false;
        Uri? rdpeNextUrl = null;

        bool firstPage = true;

        while (remaining > 0 && (useRdpe ? rdpeNextUrl is not null : currentPage <= totalPages))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Uri url;
            if (useRdpe && rdpeNextUrl is not null)
            {
                url = rdpeNextUrl;
            }
            else
            {
                var requestSize = Math.Min(pageSize, remaining);
                url = BuildPageUrl(endpointUrl, currentPage, requestSize);
            }

            OrukPage<OrukService>? page = null;
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    var reasonPhrase = response.ReasonPhrase ?? "Unknown";

                    // Log all HTTP errors with appropriate severity
                    if (statusCode == 429)
                    {
                        _logger.LogError(
                            "API rate limiting detected: HTTP {StatusCode} ({ReasonPhrase}) for page {Page} at {Url}. " +
                            "The endpoint may be throttling requests.",
                            statusCode, reasonPhrase, currentPage, url);
                    }
                    else if (statusCode >= 400 && statusCode < 500)
                    {
                        _logger.LogError(
                            "Client error: HTTP {StatusCode} ({ReasonPhrase}) for page {Page} at {Url}. " +
                            "This may indicate an authentication, authorization, or request format issue.",
                            statusCode, reasonPhrase, currentPage, url);
                    }
                    else if (statusCode >= 500)
                    {
                        _logger.LogError(
                            "Server error: HTTP {StatusCode} ({ReasonPhrase}) for page {Page} at {Url}. " +
                            "The endpoint may be experiencing issues.",
                            statusCode, reasonPhrase, currentPage, url);
                    }
                    else
                    {
                        _logger.LogError(
                            "HTTP error: {StatusCode} ({ReasonPhrase}) for page {Page} at {Url}.",
                            statusCode, reasonPhrase, currentPage, url);
                    }

                    if (firstPage)
                    {
                        _logger.LogError("Fatal: First page request failed. Aborting fetch.");
                        yield break;
                    }

                    if (useRdpe)
                    {
                        _logger.LogWarning("RDPE error fetching next page at {Url}. Stopping pagination.", url);
                        yield break;
                    }

                    _logger.LogWarning("Skipping page {Page} and continuing with next page.", currentPage);
                    currentPage++;
                    continue;
                }

                // Read response body for logging and fallback deserialization
                string responseBody = await response.Content.ReadAsStringAsync();

                try
                {
                    page = JsonSerializer.Deserialize<OrukPage<OrukService>>(responseBody);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex,
                        "JSON deserialization failed for page {Page} from {Url}. Attempting fallback with case-insensitive options.",
                        currentPage, url);

                    // Fallback: try case-insensitive deserialization
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        page = JsonSerializer.Deserialize<OrukPage<OrukService>>(responseBody, options);

                        if (page is not null)
                        {
                            _logger.LogInformation(
                                "Fallback deserialization succeeded for page {Page} from {Url}.",
                                currentPage, url);
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx,
                            "Fallback deserialization also failed for page {Page} from {Url}. Response body length: {BodyLength}",
                            currentPage, url, responseBody.Length);
                        throw;
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                                                    or System.Text.Json.JsonException)
            {
                if (firstPage)
                {
                    _logger.LogError(ex,
                        "Fatal error fetching first page from {Url}. Aborting.", url);
                    yield break;
                }

                if (useRdpe)
                {
                    _logger.LogWarning(ex,
                        "Error fetching or deserialising RDPE page from {Url}. Stopping pagination.", url);
                    yield break;
                }

                _logger.LogWarning(ex,
                    "Error fetching or deserialising page {Page} from {Url}. Skipping page.",
                    currentPage, url);
                currentPage++;
                continue;
            }

            if (page is null || page.Contents.Count == 0)
            {
                _logger.LogWarning(
                    "Page {Page} returned no contents (page is null: {IsNull}, page size requested: {RequestSize}). " +
                    "Stopping pagination. Total services yielded so far: {TotalYielded}.",
                    currentPage, page is null, useRdpe ? 0 : Math.Min(pageSize, remaining),
                    noLimit ? int.MaxValue - remaining : maxRecords - remaining);
                yield break;
            }

            // On the first page, detect which paging strategy the endpoint uses.
            if (firstPage)
            {
                firstPage = false;

                if (!string.IsNullOrEmpty(page.NextUrl))
                {
                    useRdpe = true;
                    _logger.LogInformation(
                        "RDPE paging detected for {Url}. Following next_url links.",
                        endpointUrl);
                }
                else
                {
                    totalPages = Math.Max(page.TotalPages, 1);
                    _logger.LogInformation(
                        "ORUK endpoint reports {TotalItems} total items across {TotalPages} pages (page size: {PageSize}).",
                        page.TotalItems, page.TotalPages, pageSize);
                }
            }

            if (useRdpe)
            {
                _logger.LogInformation(
                    "Successfully fetched RDPE page with {ItemCount} services from {Url}.",
                    page.Contents.Count, url);

                // Advance to the next RDPE URL; null or same URL signals end of feed.
                rdpeNextUrl = !string.IsNullOrEmpty(page.NextUrl)
                    ? new Uri(page.NextUrl)
                    : null;

                if (rdpeNextUrl is not null && rdpeNextUrl == url)
                {
                    _logger.LogInformation(
                        "RDPE feed returned the same next_url as the current URL ({Url}). " +
                        "The feed is up to date. Stopping pagination.",
                        url);
                    rdpeNextUrl = null;
                }
            }
            else
            {
                _logger.LogInformation(
                    "Successfully fetched page {CurrentPage}/{TotalPages} with {ItemCount} services from {Url}.",
                    currentPage, totalPages, page.Contents.Count, url);

                currentPage++;
            }

            foreach (var service in page.Contents)
            {
                if (remaining <= 0) yield break;
                yield return service;
                remaining--;
            }
        }

        // Log completion summary
        var totalFetched = noLimit ? int.MaxValue - remaining : maxRecords - remaining;
        _logger.LogInformation(
            "Pagination completed. Fetched {TotalFetched} services across {PagesFetched} pages from {Url}.",
            totalFetched, useRdpe ? totalFetched : currentPage - 1, endpointUrl);
    }

    private static Uri BuildPageUrl(Uri baseUrl, int page, int perPage)
    {
        var builder = new UriBuilder(baseUrl);
        var existing = System.Web.HttpUtility.ParseQueryString(builder.Query);
        existing["page"] = page.ToString();
        existing["per_page"] = perPage.ToString();
        builder.Query = existing.ToString();
        return builder.Uri;
    }
}
