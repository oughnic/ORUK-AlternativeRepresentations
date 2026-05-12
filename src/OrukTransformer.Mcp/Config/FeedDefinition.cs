namespace OrukTransformer.Mcp.Config;

public sealed record FeedDefinition(Uri Url, string? Name = null, IReadOnlyList<string>? Aliases = null)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Url.Host : Name;
}
