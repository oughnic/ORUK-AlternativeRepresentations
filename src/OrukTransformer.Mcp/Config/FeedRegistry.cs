namespace OrukTransformer.Mcp.Config;

public sealed class FeedRegistry : IFeedRegistry
{
    public FeedRegistry(IReadOnlyList<FeedDefinition> feeds)
    {
        Feeds = feeds;
        FeedUrls = feeds.Select(f => f.Url).ToList().AsReadOnly();
    }

    public IReadOnlyList<FeedDefinition> Feeds { get; }

    public IReadOnlyList<Uri> FeedUrls { get; }

    public FeedDefinition? Resolve(string? feedIdentifier)
    {
        if (string.IsNullOrWhiteSpace(feedIdentifier))
            return null;

        if (Uri.TryCreate(feedIdentifier, UriKind.Absolute, out var uri))
            return Resolve(uri);

        return Feeds.FirstOrDefault(feed =>
            string.Equals(feed.DisplayName, feedIdentifier, StringComparison.OrdinalIgnoreCase) ||
            (feed.Aliases?.Any(alias => string.Equals(alias, feedIdentifier, StringComparison.OrdinalIgnoreCase)) ?? false));
    }

    public FeedDefinition? Resolve(Uri feedUrl)
    {
        var normalised = Normalise(feedUrl);
        return Feeds.FirstOrDefault(feed => Normalise(feed.Url) == normalised);
    }

    public string? GetDisplayName(Uri feedUrl)
        => Resolve(feedUrl)?.DisplayName;

    private static string Normalise(Uri uri)
    {
        var s = uri.ToString().TrimEnd('/');
        if (s.EndsWith("/services", StringComparison.OrdinalIgnoreCase))
            s = s[..^"/services".Length];
        return s.ToLowerInvariant();
    }
}
