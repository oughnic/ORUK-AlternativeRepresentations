using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool that exposes metadata about the configured ORUK data sources.
/// Lets the AI agent inform the user which service directories are being searched
/// and make informed decisions about which feeds to query.
/// </summary>
[McpServerToolType]
public sealed class OrukFeedInfoTool(
    IReadOnlyList<Uri> feedUrls,
    ILogger<OrukFeedInfoTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "List the Open Referral UK service directory feeds that this server is configured to search. " +
        "Use this to inform the user which local authority or community directories are included, " +
        "or to check whether a specific region's data is available before searching.")]
    public string ListFeeds()
    {
        logger.LogInformation("ListFeeds: returning {Count} configured feed(s).", feedUrls.Count);

        if (feedUrls.Count == 0)
        {
            logger.LogWarning("ListFeeds: no feeds are configured.");
            return JsonSerializer.Serialize(new
            {
                feed_count = 0,
                feeds = Array.Empty<object>(),
                message = "No ORUK feeds are configured. Add endpoint URLs to feeds.json."
            }, JsonOptions);
        }

        var feeds = feedUrls.Select((uri, index) => new
        {
            index = index + 1,
            url = uri.ToString(),
            host = uri.Host
        }).ToList();

        return JsonSerializer.Serialize(new
        {
            feed_count = feeds.Count,
            feeds
        }, JsonOptions);
    }
}
