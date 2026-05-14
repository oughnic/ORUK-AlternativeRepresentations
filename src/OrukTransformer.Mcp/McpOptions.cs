namespace OrukTransformer.Mcp;

/// <summary>
/// Configuration options for the ORUK MCP server.
/// Bound from the "Mcp" section of appsettings.json.
/// Feed URLs are loaded from feeds.json at startup (see <see cref="Config.FeedsLoader"/>).
/// </summary>
public sealed class McpOptions
{
    /// <summary>
    /// Maximum number of service records to return per search query across all feeds.
    /// </summary>
    public int MaxResultsPerQuery { get; set; } = 20;

    /// <summary>
    /// How long to cache taxonomy terms per feed before refreshing, in minutes.
    /// </summary>
    public int TaxonomyCacheTtlMinutes { get; set; } = 60;
}
