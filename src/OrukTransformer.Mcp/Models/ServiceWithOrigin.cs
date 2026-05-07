using OrukModels.Models;

namespace OrukTransformer.Mcp.Models;

/// <summary>
/// Wraps a service result together with the feed it was retrieved from.
/// Needed for multi-feed fan-out so follow-up detail requests can target the
/// correct endpoint.
/// </summary>
internal sealed record ServiceWithOrigin(
    OrukService Service,
    Uri FeedBaseUrl);
