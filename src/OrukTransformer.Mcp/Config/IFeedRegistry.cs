namespace OrukTransformer.Mcp.Config;

public interface IFeedRegistry
{
    IReadOnlyList<FeedDefinition> Feeds { get; }

    IReadOnlyList<Uri> FeedUrls { get; }

    FeedDefinition? Resolve(string? feedIdentifier);

    FeedDefinition? Resolve(Uri feedUrl);

    string? GetDisplayName(Uri feedUrl);
}
