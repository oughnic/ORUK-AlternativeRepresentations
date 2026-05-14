using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OrukApiClient;
using OrukModels.Models;
using OrukTransformer.Mcp.Config;
using OrukTransformer.Mcp.Models;
using OrukTransformer.Mcp.Taxonomy;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool for searching across all configured ORUK service directory feeds.
/// The AI agent calls this tool to find services matching the user's request.
/// </summary>
[McpServerToolType]
public sealed class OrukServiceSearchTool(
    IOrukServiceClient serviceClient,
    ITaxonomyCache taxonomyCache,
    IOptions<McpOptions> options,
    IFeedRegistry feedRegistry,
    ILogger<OrukServiceSearchTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Search for community services, support groups, and public sector resources from Open " +
        "Referral UK service directories. Use this to help users find services such as health " +
        "support, childcare, sports clubs, community groups, disability support, food banks, " +
        "or social care. Returns a list of matching services with summaries and contact details.")]
    public async Task<string> SearchServices(
        [Description(
            "Keywords describing what the user is looking for, e.g. 'baby groups', " +
            "'swimming lessons', 'dementia support', 'food bank', 'after school clubs'")]
        string query,
        [Description(
            "UK postcode or place name to search near, e.g. 'BS1 4ST', 'Bristol city centre', " +
            "'Leeds'. Leave empty to search across the whole directory.")]
        string? location = null,
        [Description("Search radius in kilometres when a location is given. Default is 5.")]
        [Range(0, 50)]
        double? radiusKm = null,
        [Description("Set to true to return only free-of-charge services.")]
        bool freeOnly = false,
        [Description("Minimum age of intended service users (inclusive).")]
        [Range(0, 130)]
        double? minimumAge = null,
        [Description("Maximum age of intended service users (inclusive).")]
        [Range(0, 130)]
        double? maximumAge = null,
        [Description(
            "Restrict results to a specific feed URL, feed name, or alias (from list_feeds). " +
            "If omitted, all configured feeds are searched.")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "SearchServices: query='{Query}', location='{Location}', radius={Radius}km, " +
            "freeOnly={FreeOnly}, minAge={MinAge}, maxAge={MaxAge}, feedUrl='{FeedUrl}', feeds={FeedCount}.",
            query, location ?? "(any)", radiusKm, freeOnly, minimumAge, maximumAge,
            feedUrl ?? "(all)", feedRegistry.Feeds.Count);

        if (feedRegistry.Feeds.Count == 0)
        {
            logger.LogError("SearchServices aborted: no ORUK feeds configured.");
            return """{"error":"No ORUK feeds are configured. Add feed URLs to feeds.json."}""";
        }

        var targets = ResolveFeedTargets(feedUrl);
        var maxTotal = options.Value.MaxResultsPerQuery;
        var results = new List<ServiceWithOrigin>();

        // Each feed independently fetches up to maxTotal records so client-side filters
        // have sufficient data to work with; the final .Take(maxTotal) limits what is returned.
        var perFeedLimit = maxTotal;

        var tasks = targets.Select(async targetFeed =>
        {
            try
            {
                var termIds = await TryResolveTermIds(query, targetFeed.Url, cancellationToken);

                if (termIds.Count > 0)
                    logger.LogInformation(
                        "Taxonomy resolved '{Query}' to {Count} term(s) for feed {FeedUrl}.",
                        query, termIds.Count, targetFeed.Url);
                else
                    logger.LogInformation(
                        "No taxonomy match for '{Query}' in feed {FeedUrl} — using keyword search only.",
                        query, targetFeed.Url);

                var searchQuery = new OrukServiceQuery
                {
                    Keyword = query,
                    TaxonomyTermIds = termIds,
                    Proximity = location,
                    RadiusKm = radiusKm,
                    FreeOnly = freeOnly,
                    MinimumAge = minimumAge,
                    MaximumAge = maximumAge,
                    MaxRecords = perFeedLimit
                };

                var feedResults = new List<ServiceWithOrigin>();
                await foreach (var service in serviceClient.SearchAsync(targetFeed.Url, searchQuery, cancellationToken))
                {
                    feedResults.Add(new ServiceWithOrigin(service, targetFeed));
                }

                logger.LogInformation(
                    "Feed {FeedUrl} returned {Count} result(s) for query '{Query}'.",
                    targetFeed.Url, feedResults.Count, query);

                return feedResults;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Search failed for feed {FeedUrl}. Skipping this feed.", targetFeed.Url);
                return new List<ServiceWithOrigin>();
            }
        });

        var allResults = await Task.WhenAll(tasks);
        foreach (var batch in allResults)
            results.AddRange(batch);

        // De-duplicate services that appear in multiple feeds (by service ID)
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = results
            .Where(r => seen.Add(r.Service.Id))
            .Take(maxTotal)
            .ToList();

        if (unique.Count == 0)
            logger.LogWarning(
                "SearchServices: no results found for query '{Query}' across {FeedCount} feed(s).",
                query, feedRegistry.Feeds.Count);
        else
            logger.LogInformation(
                "SearchServices: returning {Unique} unique service(s) (from {Total} raw result(s)) for query '{Query}'.",
                unique.Count, results.Count, query);

        var summaries = unique.Select(r => MapToSummary(r)).ToList();
        return JsonSerializer.Serialize(new { count = summaries.Count, services = summaries }, JsonOptions);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private List<FeedDefinition> ResolveFeedTargets(string? feedUrl)
    {
        if (string.IsNullOrWhiteSpace(feedUrl))
            return feedRegistry.Feeds.ToList();

        var match = feedRegistry.Resolve(feedUrl);

        if (match is null)
        {
            logger.LogWarning(
                "SearchServices: feedUrl '{FeedUrl}' not found in configured feeds — searching all feeds.",
                feedUrl);
            return feedRegistry.Feeds.ToList();
        }

        return [match];
    }

    private async Task<IReadOnlyList<string>> TryResolveTermIds(
        string query, Uri feedUrl, CancellationToken cancellationToken)
    {
        try
        {
            return await taxonomyCache.ResolveAsync(query, feedUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Taxonomy resolution failed for '{Query}' against {FeedUrl} — falling back to keyword search only.",
                query, feedUrl);
            return [];
        }
    }

    private static object MapToSummary(ServiceWithOrigin r)
    {
        var s = r.Service;
        var location = s.ServiceAtLocations
            .Select(sal => sal.Location)
            .FirstOrDefault(l => l is not null);

        var physicalAddr = location?.PhysicalAddresses.FirstOrDefault();

        return new
        {
            id = s.Id,
            feed_url = r.FeedBaseUrl.ToString(),
            feed_name = r.FeedName,
            name = s.Name,
            description = string.IsNullOrWhiteSpace(s.Description)
                ? null
                : TruncateDescription(s.Description, 200),
            status = s.Status,
            url = s.Url,
            email = s.Email,
            phone = s.Phones.Select(p => p.Number).FirstOrDefault(),
            organization = s.Organization?.Name,
            location = location is null ? null : new
            {
                name = location.Name,
                address = FormatAddress(physicalAddr),
                postcode = physicalAddr?.PostalCode
            },
            age_range = s.MinimumAge.HasValue || s.MaximumAge.HasValue
                ? $"{s.MinimumAge?.ToString() ?? "any"}–{s.MaximumAge?.ToString() ?? "any"}"
                : null,
            free = s.CostOptions.Count == 0 ? (bool?)true : null
        };
    }

    private static string? FormatAddress(OrukAddress? addr)
    {
        if (addr is null) return null;
        var parts = new[] { addr.Address1, addr.City, addr.StateProvince, addr.PostalCode }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private static string TruncateDescription(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength].TrimEnd() + "…";
}
