using OrukModels.Models;

namespace OrukApiClient;

/// <summary>
/// Client for the ORUK v3 /taxonomy_terms endpoint.
/// Provides taxonomy term retrieval and label-to-ID resolution.
/// </summary>
public interface IOrukTaxonomyClient
{
    /// <summary>
    /// Returns all taxonomy terms published by the feed, including parent–child
    /// relationships. Paginates automatically until all terms are retrieved.
    /// </summary>
    /// <param name="feedBaseUrl">The base URL of the ORUK v3 API.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<OrukTaxonomyTerm>> GetAllTermsAsync(
        Uri feedBaseUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a human-readable label to matching taxonomy term IDs from a
    /// pre-loaded term list. Applies exact match, then partial/substring match,
    /// then parent-term expansion. Returns an empty list if no match is found.
    /// </summary>
    /// <param name="label">The label to resolve, e.g. "baby groups" or "dementia".</param>
    /// <param name="terms">Pre-loaded term list from <see cref="GetAllTermsAsync"/>.</param>
    IReadOnlyList<string> ResolveByLabel(string label, IReadOnlyList<OrukTaxonomyTerm> terms);
}
