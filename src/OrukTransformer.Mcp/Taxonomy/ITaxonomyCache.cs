using OrukModels.Models;

namespace OrukTransformer.Mcp.Taxonomy;

/// <summary>
/// Provides cached access to taxonomy terms for each configured feed.
/// Refreshes automatically when the TTL expires.
/// </summary>
public interface ITaxonomyCache
{
    /// <summary>
    /// Returns the taxonomy terms for the given feed, loading and caching
    /// them on first access (or after the TTL expires).
    /// </summary>
    Task<IReadOnlyList<OrukTaxonomyTerm>> GetTermsAsync(
        Uri feedBaseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a human-readable label to matching taxonomy term IDs from the
    /// specified feed's cached term list.
    /// </summary>
    Task<IReadOnlyList<string>> ResolveAsync(
        string label,
        Uri feedBaseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Clears all cached terms, forcing a refresh on next access.</summary>
    void InvalidateAll();
}
