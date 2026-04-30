using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OrukModels.Models;

namespace OrukTransformer.Cli.Fetching;

/// <summary>
/// Fetches ORUK service records by iterating the paged <c>GET /services</c>
/// endpoint, using query parameters <c>page</c> and <c>per_page</c> to
/// minimise the number of HTTP round-trips while respecting endpoint load.
/// </summary>
/// <remarks>
/// Page size is capped at 100 records per request, or at
/// <paramref name="maxRecords"/> when a smaller positive limit is requested.
/// If an individual page fails to deserialise, a warning is logged and that
/// page is skipped; the fetch continues with the next page ("receive liberally").
/// If the <em>first</em> page returns a non-success HTTP status code the fetch
/// is aborted immediately.
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

        int totalPages = 1;
        int currentPage = 1;
        bool firstPage = true;

        while (currentPage <= totalPages && remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestSize = Math.Min(pageSize, remaining);
            var url = BuildPageUrl(endpointUrl, currentPage, requestSize);

            OrukPage<OrukService>? page = null;
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (firstPage)
                    {
                        _logger.LogError(
                            "ORUK endpoint returned {StatusCode} for URL {Url}. Aborting fetch.",
                            (int)response.StatusCode, url);
                        yield break;
                    }

                    _logger.LogWarning(
                        "ORUK endpoint returned {StatusCode} for page {Page} at {Url}. Skipping page.",
                        (int)response.StatusCode, currentPage, url);
                    currentPage++;
                    continue;
                }

                page = await response.Content.ReadFromJsonAsync<OrukPage<OrukService>>(
                    cancellationToken: cancellationToken);
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

                _logger.LogWarning(ex,
                    "Error fetching or deserialising page {Page} from {Url}. Skipping page.",
                    currentPage, url);
                currentPage++;
                continue;
            }

            if (page is null || page.Contents.Count == 0)
            {
                _logger.LogDebug("Page {Page} returned no contents. Stopping.", currentPage);
                yield break;
            }

            // Update total pages from the first response
            if (firstPage)
            {
                totalPages = Math.Max(page.TotalPages, 1);
                firstPage = false;
                _logger.LogDebug(
                    "ORUK endpoint reports {TotalItems} total items across {TotalPages} pages.",
                    page.TotalItems, page.TotalPages);
            }

            foreach (var service in page.Contents)
            {
                if (remaining <= 0) yield break;
                yield return service;
                remaining--;
            }

            currentPage++;
        }
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
