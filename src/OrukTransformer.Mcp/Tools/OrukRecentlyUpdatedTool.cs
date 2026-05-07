using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OrukApiClient;
using OrukTransformer.Mcp.Models;
using OrukTransformer.Mcp.Taxonomy;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool for discovering recently added or updated ORUK services.
/// Supports monitoring workflows and helps agents avoid recommending stale data.
/// </summary>
[McpServerToolType]
public sealed class OrukRecentlyUpdatedTool(
    IOrukServiceClient serviceClient,
    ITaxonomyCache taxonomyCache,
    IOptions<McpOptions> options,
    IReadOnlyList<Uri> feedUrls,
    ILogger<OrukRecentlyUpdatedTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Find Open Referral UK services that have been added or updated since a given date. " +
        "Use this when the user asks about new or recently changed services — for example " +
        "'What new carer support services have been added this month?' or " +
        "'Have any food banks opened recently near me?'. " +
        "Services with no modification date recorded are excluded from results.")]
    public async Task<string> GetServicesUpdatedSince(
        [Description(
            "The earliest modification date to include, in ISO 8601 format (e.g. '2025-01-01', " +
            "'2025-03-15T00:00:00Z'). Services last modified on or after this date are returned.")]
        string sinceDate,
        [Description("Optional keyword to further narrow results, e.g. 'dementia', 'food bank'.")]
        string? keyword = null,
        [Description("UK postcode or place name to search near.")]
        string? location = null,
        [Description("Search radius in kilometres when a location is given.")]
        double? radiusKm = null,
        [Description("Set to true to return only free-of-charge services.")]
        bool freeOnly = false,
        [Description("Restrict results to a specific feed URL — obtained from list_feeds. " +
                     "If omitted, all configured feeds are searched.")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GetServicesUpdatedSince: since='{Since}', keyword='{Keyword}', location='{Location}', feedUrl='{FeedUrl}'.",
            sinceDate, keyword ?? "(any)", location ?? "(any)", feedUrl ?? "(all feeds)");

        if (feedUrls.Count == 0)
        {
            logger.LogError("GetServicesUpdatedSince: no ORUK feeds configured.");
            return JsonSerializer.Serialize(new { error = "No ORUK feeds are configured." });
        }

        if (!DateTimeOffset.TryParse(sinceDate, out var since))
        {
            logger.LogWarning("GetServicesUpdatedSince: could not parse since_date '{Since}'.", sinceDate);
            return JsonSerializer.Serialize(new
            {
                error = $"Could not parse since_date '{sinceDate}'. Use ISO 8601 format, e.g. '2025-01-01'."
            });
        }

        var targets = ResolveFeedTargets(feedUrl);
        var maxTotal = options.Value.MaxResultsPerQuery;
        var perFeedLimit = Math.Max(1, (int)Math.Ceiling((double)maxTotal / targets.Count));

        var tasks = targets.Select(async targetFeedUrl =>
        {
            try
            {
                IReadOnlyList<string> termIds = [];
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    try
                    {
                        termIds = await taxonomyCache.ResolveAsync(keyword, targetFeedUrl, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Taxonomy resolution failed for '{Keyword}' against {FeedUrl}.",
                            keyword, targetFeedUrl);
                    }
                }

                var query = new OrukServiceQuery
                {
                    Keyword = keyword,
                    TaxonomyTermIds = termIds,
                    Proximity = location,
                    RadiusKm = radiusKm,
                    FreeOnly = freeOnly,
                    UpdatedSince = since,
                    // Fetch more pages than usual — date filtering is client-side so many
                    // records may be filtered out before reaching MaxRecords.
                    MaxRecords = perFeedLimit * 5
                };

                var feedResults = new List<ServiceWithOrigin>();
                await foreach (var service in serviceClient.SearchAsync(targetFeedUrl, query, cancellationToken))
                    feedResults.Add(new ServiceWithOrigin(service, targetFeedUrl));

                logger.LogInformation(
                    "GetServicesUpdatedSince: feed {FeedUrl} returned {Count} result(s) updated since {Since}.",
                    targetFeedUrl, feedResults.Count, sinceDate);

                return feedResults;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Search failed for feed {FeedUrl}. Skipping.", targetFeedUrl);
                return new List<ServiceWithOrigin>();
            }
        });

        var allResults = await Task.WhenAll(tasks);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = allResults
            .SelectMany(r => r)
            .Where(r => seen.Add(r.Service.Id))
            .OrderByDescending(r => r.Service.LastModified)
            .Take(maxTotal)
            .ToList();

        if (unique.Count == 0)
            logger.LogWarning(
                "GetServicesUpdatedSince: no services found updated since '{Since}'.", sinceDate);
        else
            logger.LogInformation(
                "GetServicesUpdatedSince: returning {Count} service(s) updated since '{Since}'.",
                unique.Count, sinceDate);

        var summaries = unique.Select(r =>
        {
            var s = r.Service;
            var loc = s.ServiceAtLocations.Select(sal => sal.Location).FirstOrDefault(l => l is not null);
            var addr = loc?.PhysicalAddresses.FirstOrDefault();
            return new
            {
                id = s.Id,
                feed_url = r.FeedBaseUrl.ToString(),
                name = s.Name,
                description = s.Description is { Length: > 0 }
                    ? (s.Description.Length > 200 ? s.Description[..200].TrimEnd() + "…" : s.Description)
                    : null,
                status = s.Status,
                last_modified = s.LastModified,
                url = s.Url,
                organization = s.Organization?.Name,
                location = loc is null ? null : new
                {
                    name = loc.Name,
                    address = addr is null
                        ? null
                        : string.Join(", ",
                            new[] { addr.Address1, addr.City, addr.StateProvince, addr.PostalCode }
                            .Where(p => !string.IsNullOrWhiteSpace(p))),
                    postcode = addr?.PostalCode
                }
            };
        }).ToList();

        return JsonSerializer.Serialize(new
        {
            count = summaries.Count,
            since = sinceDate,
            services = summaries
        }, JsonOptions);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private List<Uri> ResolveFeedTargets(string? feedUrl)
    {
        if (string.IsNullOrWhiteSpace(feedUrl))
            return feedUrls.ToList();

        if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var uri))
        {
            logger.LogWarning("GetServicesUpdatedSince: invalid feedUrl '{FeedUrl}' — searching all feeds.", feedUrl);
            return feedUrls.ToList();
        }

        var normalised = NormaliseUrl(uri);
        var match = feedUrls.Where(f => NormaliseUrl(f) == normalised).ToList();

        if (match.Count == 0)
        {
            logger.LogWarning(
                "GetServicesUpdatedSince: feedUrl '{FeedUrl}' not in configured feeds — searching all feeds.",
                feedUrl);
            return feedUrls.ToList();
        }

        return match;
    }

    private static string NormaliseUrl(Uri uri)
    {
        var s = uri.ToString().TrimEnd('/');
        if (s.EndsWith("/services", StringComparison.OrdinalIgnoreCase))
            s = s[..^"/services".Length];
        return s.ToLowerInvariant();
    }
}
