using OrukModels.Models;

namespace OrukApiClient;

/// <summary>
/// Client for querying an ORUK v3 /organizations endpoint.
/// Supports paginated search and single-record retrieval.
/// </summary>
public interface IOrukOrganizationClient
{
    /// <summary>
    /// Search for organisations matching the optional keyword against the specified feed.
    /// Paginates automatically and yields results as they arrive.
    /// </summary>
    /// <param name="feedBaseUrl">
    /// The base URL of the ORUK v3 API, e.g. <c>https://example.org/o/OpenReferralService/v3</c>.
    /// The <c>/organizations</c> path is appended automatically.
    /// </param>
    /// <param name="keyword">
    /// Optional keyword to filter organisations by name or description.
    /// Pass <see langword="null"/> to return all organisations.
    /// </param>
    /// <param name="maxRecords">
    /// Maximum number of organisations to return across all pages. Use 0 for no limit.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    IAsyncEnumerable<OrukOrganization> SearchAsync(
        Uri feedBaseUrl,
        string? keyword = null,
        int maxRecords = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full record for a single organisation by its ID.
    /// Returns <see langword="null"/> if the organisation does not exist (404).
    /// </summary>
    /// <param name="feedBaseUrl">The base URL of the ORUK v3 API.</param>
    /// <param name="organizationId">The ORUK organisation ID (UUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<OrukOrganization?> GetByIdAsync(
        Uri feedBaseUrl,
        string organizationId,
        CancellationToken cancellationToken = default);
}
