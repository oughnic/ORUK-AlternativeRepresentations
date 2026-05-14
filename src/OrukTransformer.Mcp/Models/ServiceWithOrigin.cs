using OrukModels.Models;
using OrukTransformer.Mcp.Config;

namespace OrukTransformer.Mcp.Models;

/// <summary>
/// Wraps a service result together with the feed it was retrieved from.
/// Needed for multi-feed fan-out so follow-up detail requests can target the
/// correct endpoint.
/// </summary>
internal sealed record ServiceWithOrigin(
    OrukService Service,
    FeedDefinition Feed)
{
    public Uri FeedBaseUrl => Feed.Url;

    public string FeedName => Feed.DisplayName;
}
