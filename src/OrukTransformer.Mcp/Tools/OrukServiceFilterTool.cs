using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OrukApiClient;
using OrukModels.Models;
using OrukTransformer.Mcp;
using OrukTransformer.Mcp.Config;
using OrukTransformer.Mcp.Models;
using OrukTransformer.Mcp.Taxonomy;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tools for filtering ORUK services by language of delivery, accessibility
/// features, and delivery type (physical / virtual / postal).
/// All three tools fan out across all configured feeds and apply client-side filters.
/// </summary>
[McpServerToolType]
public sealed class OrukServiceFilterTool(
    IOrukServiceClient serviceClient,
    ITaxonomyCache taxonomyCache,
    IOptions<McpOptions> options,
    IFeedRegistry feedRegistry,
    ILogger<OrukServiceFilterTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Find Open Referral UK services that are delivered in a specific language — for example " +
        "Welsh, Polish, Arabic, Somali, British Sign Language (BSL), or Cantonese. " +
        "Use this when the user or their family member does not speak English fluently, " +
        "or when the user specifically asks for services in their own language.")]
    public async Task<string> GetServicesByLanguage(
        [Description("The language to filter by, e.g. 'Welsh', 'Polish', 'Arabic', 'BSL', 'Cantonese'. " +
                     "ISO 639 codes (e.g. 'cy', 'pl') are also accepted.")]
        string language,
        [Description("Optional keyword to further narrow results, e.g. 'dementia support', 'childcare'.")]
        string? keyword = null,
        [Description("UK postcode or place name to search near.")]
        string? location = null,
        [Description("Search radius in kilometres when a location is given. Default is 5.")]
        [Range(0, 50)]
        double? radiusKm = null,
        [Description("Restrict results to a specific feed URL, feed name, or alias — obtained from list_feeds. " +
                      "If omitted, all configured feeds are searched.")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GetServicesByLanguage: language='{Language}', keyword='{Keyword}', location='{Location}', feedUrl='{FeedUrl}'.",
            language, keyword ?? "(any)", location ?? "(any)", feedUrl ?? "(all feeds)");

        if (feedRegistry.Feeds.Count == 0)
        {
            logger.LogError("GetServicesByLanguage: no ORUK feeds configured.");
            return JsonSerializer.Serialize(new { error = "No ORUK feeds are configured." });
        }

        var query = new OrukServiceQuery
        {
            Keyword = keyword,
            Proximity = location,
            RadiusKm = radiusKm,
            Language = language,
            MaxRecords = options.Value.MaxResultsPerQuery
        };

        var targets = ResolveFeedTargets(feedUrl);
        var results = await FanOutSearchAsync(query, targets, cancellationToken);

        logger.LogInformation(
            "GetServicesByLanguage: '{Language}' returned {Count} result(s).", language, results.Count);

        if (results.Count == 0)
            logger.LogWarning(
                "GetServicesByLanguage: no services found for language '{Language}'.", language);

        return SerialiseResults(results);
    }

    [McpServerTool]
    [Description(
        "Find Open Referral UK services that have a specific accessibility feature at their location — " +
        "such as wheelchair access, hearing loop, accessible parking, BSL interpreter, " +
        "or accessible toilets. Use this for users with mobility, sensory, or cognitive impairments, " +
        "or when a carer is looking for accessible venues for someone they support.")]
    public async Task<string> FindAccessibleServices(
        [Description("The accessibility feature to filter by, e.g. 'wheelchair', 'hearing loop', " +
                     "'accessible parking', 'BSL', 'step-free'.")]
        string accessibilityFeature,
        [Description("Optional keyword to further narrow results, e.g. 'swimming', 'social group'.")]
        string? keyword = null,
        [Description("UK postcode or place name to search near.")]
        string? location = null,
        [Description("Search radius in kilometres when a location is given. Default is 5.")]
        [Range(0, 50)]
        double? radiusKm = null,
        [Description("Restrict results to a specific feed URL, feed name, or alias — obtained from list_feeds. " +
                      "If omitted, all configured feeds are searched.")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "FindAccessibleServices: feature='{Feature}', keyword='{Keyword}', location='{Location}', feedUrl='{FeedUrl}'.",
            accessibilityFeature, keyword ?? "(any)", location ?? "(any)", feedUrl ?? "(all feeds)");

        if (feedRegistry.Feeds.Count == 0)
        {
            logger.LogError("FindAccessibleServices: no ORUK feeds configured.");
            return JsonSerializer.Serialize(new { error = "No ORUK feeds are configured." });
        }

        var query = new OrukServiceQuery
        {
            Keyword = keyword,
            Proximity = location,
            RadiusKm = radiusKm,
            AccessibilityFeature = accessibilityFeature,
            MaxRecords = options.Value.MaxResultsPerQuery
        };

        var targets = ResolveFeedTargets(feedUrl);
        var results = await FanOutSearchAsync(query, targets, cancellationToken);

        logger.LogInformation(
            "FindAccessibleServices: '{Feature}' returned {Count} result(s).",
            accessibilityFeature, results.Count);

        if (results.Count == 0)
            logger.LogWarning(
                "FindAccessibleServices: no services found with feature '{Feature}'.",
                accessibilityFeature);

        return SerialiseResults(results);
    }

    [McpServerTool]
    [Description(
        "Find Open Referral UK services filtered by how they are delivered: " +
        "in-person at a physical location, online/virtual (video or phone), or by post. " +
        "Use 'physical' for face-to-face services, 'virtual' for online or telephone services, " +
        "and 'postal' for services delivered by mail. " +
        "Particularly useful when a user cannot travel or specifically needs remote access.")]
    public async Task<string> FindServicesByDeliveryType(
        [Description("The delivery type: 'physical' (in-person), 'virtual' (online/phone), or 'postal'.")]
        [AllowedValues("physical", "virtual", "postal")]
        string deliveryType,
        [Description("Optional keyword to further narrow results, e.g. 'counselling', 'food bank'.")]
        string? keyword = null,
        [Description("UK postcode or place name to search near (relevant for physical services).")]
        string? location = null,
        [Description("Search radius in kilometres when a location is given.")]
        [Range(0, 50)]
        double? radiusKm = null,
        [Description("Set to true to return only free-of-charge services.")]
        bool freeOnly = false,
        [Description("Restrict results to a specific feed URL, feed name, or alias — obtained from list_feeds. " +
                      "If omitted, all configured feeds are searched.")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "FindServicesByDeliveryType: type='{Type}', keyword='{Keyword}', location='{Location}', freeOnly={FreeOnly}, feedUrl='{FeedUrl}'.",
            deliveryType, keyword ?? "(any)", location ?? "(any)", freeOnly, feedUrl ?? "(all feeds)");

        if (feedRegistry.Feeds.Count == 0)
        {
            logger.LogError("FindServicesByDeliveryType: no ORUK feeds configured.");
            return JsonSerializer.Serialize(new { error = "No ORUK feeds are configured." });
        }

        var normalised = deliveryType.Trim().ToLowerInvariant();
        if (normalised is not ("physical" or "virtual" or "postal"))
        {
            logger.LogWarning(
                "FindServicesByDeliveryType: unrecognised delivery type '{Type}' — proceeding anyway.",
                deliveryType);
        }

        var query = new OrukServiceQuery
        {
            Keyword = keyword,
            Proximity = location,
            RadiusKm = radiusKm,
            FreeOnly = freeOnly,
            DeliveryType = normalised,
            MaxRecords = options.Value.MaxResultsPerQuery
        };

        var targets = ResolveFeedTargets(feedUrl);
        var results = await FanOutSearchAsync(query, targets, cancellationToken);

        logger.LogInformation(
            "FindServicesByDeliveryType: '{Type}' returned {Count} result(s).", deliveryType, results.Count);

        if (results.Count == 0)
            logger.LogWarning(
                "FindServicesByDeliveryType: no '{Type}' services found.", deliveryType);

        return SerialiseResults(results);
    }

    // ── Shared helpers ────────────────────────────────────────────────────────────

    private List<FeedDefinition> ResolveFeedTargets(string? feedUrl)
    {
        if (string.IsNullOrWhiteSpace(feedUrl))
            return feedRegistry.Feeds.ToList();

        var match = feedRegistry.Resolve(feedUrl);
        if (match is null)
        {
            logger.LogWarning("feedUrl '{FeedUrl}' not found in configured feeds — searching all feeds.", feedUrl);
            return feedRegistry.Feeds.ToList();
        }

        return [match];
    }

    private async Task<List<ServiceWithOrigin>> FanOutSearchAsync(
        OrukServiceQuery query, List<FeedDefinition> targets, CancellationToken cancellationToken)
    {
        var maxTotal = options.Value.MaxResultsPerQuery;
        var perFeedLimit = maxTotal;
        var cappedQuery = query with { MaxRecords = perFeedLimit };

        var tasks = targets.Select(async targetFeed =>
        {
            try
            {
                // Attempt taxonomy resolution if a keyword was supplied
                IReadOnlyList<string> termIds = [];
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    try
                    {
                        termIds = await taxonomyCache.ResolveAsync(query.Keyword, targetFeed.Url, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Taxonomy resolution failed for '{Keyword}' against {FeedUrl}.",
                            query.Keyword, targetFeed.Url);
                    }
                }

                var resolvedQuery = cappedQuery with { TaxonomyTermIds = termIds };
                var feedResults = new List<ServiceWithOrigin>();

                await foreach (var service in serviceClient.SearchAsync(targetFeed.Url, resolvedQuery, cancellationToken))
                    feedResults.Add(new ServiceWithOrigin(service, targetFeed));

                return feedResults;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Search failed for feed {FeedUrl}. Skipping.", targetFeed.Url);
                return new List<ServiceWithOrigin>();
            }
        });

        var allResults = await Task.WhenAll(tasks);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return allResults
            .SelectMany(r => r)
            .Where(r => seen.Add(r.Service.Id))
            .Take(maxTotal)
            .ToList();
    }

    private string SerialiseResults(List<ServiceWithOrigin> results)
    {
        var summaries = results.Select(r =>
        {
            var s = r.Service;
            var loc = s.ServiceAtLocations.Select(sal => sal.Location).FirstOrDefault(l => l is not null);
            var addr = loc?.PhysicalAddresses.FirstOrDefault();
            var languages = s.Languages
                .Select(l => PlainTextSanitizer.ToPlainText(l.Name ?? l.Code))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();
            var accessibility = s.ServiceAtLocations
                .Select(sal => sal.Location)
                .Where(l => l is not null)
                .SelectMany(l => l!.Accessibility)
                .Select(a => PlainTextSanitizer.ToPlainText(a.Description))
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .ToList();

            return new
            {
                id = s.Id,
                feed_url = r.FeedBaseUrl.ToString(),
                feed_name = r.FeedName,
                name = s.Name,
                description = PlainTextSanitizer.ToPlainTextAndTruncate(s.Description, 200),
                status = s.Status,
                url = s.Url,
                email = s.Email,
                phone = s.Phones.Select(p => p.Number).FirstOrDefault(),
                organization = s.Organization?.Name,
                location = loc is null ? null : new
                {
                    name = loc.Name,
                    type = loc.LocationType,
                    address = addr is null
                        ? null
                        : string.Join(", ",
                            new[] { addr.Address1, addr.City, addr.StateProvince, addr.PostalCode }
                            .Where(p => !string.IsNullOrWhiteSpace(p))),
                    postcode = addr?.PostalCode
                },
                languages = languages.Count > 0 ? languages : null,
                accessibility = accessibility.Count > 0 ? accessibility : null
            };
        }).ToList();

        return JsonSerializer.Serialize(
            new { count = summaries.Count, services = summaries }, JsonOptions);
    }
}
