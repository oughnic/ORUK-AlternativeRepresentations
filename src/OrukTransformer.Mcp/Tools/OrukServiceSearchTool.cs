using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OrukApiClient;
using OrukModels.Models;
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
    IReadOnlyList<Uri> feedUrls,
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
        double? radiusKm = null,
        [Description("Set to true to return only free-of-charge services.")]
        bool freeOnly = false,
        [Description("Minimum age of intended service users (inclusive).")]
        double? minimumAge = null,
        [Description("Maximum age of intended service users (inclusive).")]
        double? maximumAge = null,
        CancellationToken cancellationToken = default)
    {
        if (feedUrls.Count == 0)
        {
            return """{"error":"No ORUK feeds are configured. Add feed URLs to feeds.json."}""";
        }

        var maxTotal = options.Value.MaxResultsPerQuery;
        var results = new List<ServiceWithOrigin>();

        // Fan out search across all configured feeds in parallel
        var perFeedLimit = Math.Max(1, (int)Math.Ceiling((double)maxTotal / feedUrls.Count));

        var tasks = feedUrls.Select(async feedUrl =>
        {
            try
            {
                // Attempt taxonomy-based filtering first
                var termIds = await TryResolveTermIds(query, feedUrl, cancellationToken);

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
                await foreach (var service in serviceClient.SearchAsync(feedUrl, searchQuery, cancellationToken))
                {
                    feedResults.Add(new ServiceWithOrigin(service, feedUrl));
                }

                return feedResults;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Search failed for feed {FeedUrl}. Skipping.", feedUrl);
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

        var summaries = unique.Select(r => MapToSummary(r)).ToList();
        return JsonSerializer.Serialize(new { count = summaries.Count, services = summaries }, JsonOptions);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<string>> TryResolveTermIds(
        string query, Uri feedUrl, CancellationToken cancellationToken)
    {
        try
        {
            return await taxonomyCache.ResolveAsync(query, feedUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Taxonomy resolution failed for '{Query}' — falling back to keyword only.", query);
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
