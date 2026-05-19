using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OrukTransformer.Mcp;
using OrukTransformer.Mcp.Config;
using OrukTransformer.Mcp.Taxonomy;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool for browsing and resolving ORUK taxonomy terms.
/// The AI agent can use this to translate user language into taxonomy IDs
/// before calling <see cref="OrukServiceSearchTool.SearchServices"/>.
/// </summary>
[McpServerToolType]
public sealed class OrukTaxonomyTool(
    ITaxonomyCache taxonomyCache,
    IFeedRegistry feedRegistry,
    ILogger<OrukTaxonomyTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "List the taxonomy terms (service categories) available in an Open Referral UK feed. " +
        "Use this to discover the categories used in a directory — helpful for mapping the " +
        "user's language to the correct taxonomy before searching. If feed_url is omitted, " +
        "returns terms from the first configured feed.")]
    public async Task<string> ListTaxonomyTerms(
        [Description("The base URL, feed name, or alias of the ORUK feed to query (optional — defaults to the first configured feed).")]
        string? feedUrl = null,
        [Description("Optional filter: only return terms whose name or description contains this text.")]
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var feedUri = ResolveFeedUri(feedUrl);
        if (feedUri is null)
        {
            logger.LogError("ListTaxonomyTerms: no feed URL supplied and no feeds are configured.");
            return """{"error":"No ORUK feeds are configured and no feed_url was supplied."}""";
        }

        logger.LogInformation(
            "ListTaxonomyTerms: feed={FeedUrl}, filter='{Filter}'.",
            feedUri, filter ?? "(none)");

        var terms = await taxonomyCache.GetTermsAsync(feedUri, cancellationToken);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var lowerFilter = filter.ToLowerInvariant();
            terms = terms
                .Where(t =>
                    (t.Name?.Contains(lowerFilter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (PlainTextSanitizer.ToPlainText(t.Description)
                        ?.Contains(lowerFilter, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList()
                .AsReadOnly();

            if (terms.Count == 0)
                logger.LogWarning(
                    "ListTaxonomyTerms: filter '{Filter}' matched no taxonomy terms in {FeedUrl}.",
                    filter, feedUri);
        }

        var result = terms.Select(t => new
        {
            id = t.Id,
            name = t.Name,
            code = t.Code,
            taxonomy = t.Taxonomy,
            parent_id = t.ParentId,
            description = PlainTextSanitizer.ToPlainText(t.Description)
        }).ToList();

        logger.LogInformation(
            "ListTaxonomyTerms: returning {Count} term(s) from {FeedUrl}.",
            result.Count, feedUri);

        return JsonSerializer.Serialize(new { count = result.Count, terms = result }, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Resolve a plain-language label (e.g. 'dementia support', 'baby groups', 'food bank') " +
        "to matching taxonomy term IDs in an ORUK feed. The AI agent should use this before " +
        "calling search_services to improve result relevance. Returns term IDs that can be " +
        "passed implicitly through search_services — search_services performs this resolution " +
        "automatically, but this tool lets the agent inspect and explain the mapping.")]
    public async Task<string> ResolveTaxonomyLabel(
        [Description("The plain-language label to look up, e.g. 'swimming lessons', 'carer support'.")]
        string label,
        [Description("The base URL, feed name, or alias of the ORUK feed to search within (optional — defaults to first feed).")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            logger.LogWarning("ResolveTaxonomyLabel: empty label supplied.");
            return """{"error":"label must not be empty."}""";
        }

        var feedUri = ResolveFeedUri(feedUrl);
        if (feedUri is null)
        {
            logger.LogError("ResolveTaxonomyLabel: no feed URL supplied and no feeds are configured.");
            return """{"error":"No ORUK feeds are configured and no feed_url was supplied."}""";
        }

        logger.LogInformation(
            "ResolveTaxonomyLabel: label='{Label}', feed={FeedUrl}.", label, feedUri);

        var termIds = await taxonomyCache.ResolveAsync(label, feedUri, cancellationToken);
        var allTerms = await taxonomyCache.GetTermsAsync(feedUri, cancellationToken);

        var matched = allTerms
            .Where(t => termIds.Contains(t.Id, StringComparer.OrdinalIgnoreCase))
            .Select(t => new { t.Id, t.Name, t.Code, t.Taxonomy })
            .ToList();

        if (matched.Count == 0)
            logger.LogWarning(
                "ResolveTaxonomyLabel: no taxonomy terms matched '{Label}' in {FeedUrl}.",
                label, feedUri);
        else
            logger.LogInformation(
                "ResolveTaxonomyLabel: '{Label}' matched {Count} term(s) in {FeedUrl}: {Terms}.",
                label, matched.Count, feedUri,
                string.Join(", ", matched.Select(t => t.Name)));

        return JsonSerializer.Serialize(new
        {
            label,
            matched_count = matched.Count,
            terms = matched
        }, JsonOptions);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private Uri? ResolveFeedUri(string? feedUrl)
    {
        if (!string.IsNullOrWhiteSpace(feedUrl))
        {
            var configured = feedRegistry.Resolve(feedUrl);
            if (configured is not null)
                return configured.Url;

            if (Uri.TryCreate(feedUrl, UriKind.Absolute, out var supplied))
                return supplied;
        }

        return feedRegistry.Feeds.Count > 0 ? feedRegistry.Feeds[0].Url : null;
    }
}
