using OrukModels.Models;

namespace OrukApiClient;

/// <summary>
/// Client for querying an ORUK v3 /services endpoint.
/// Handles pagination, query parameter construction, and tolerant error handling.
/// </summary>
public interface IOrukServiceClient
{
    /// <summary>
    /// Search for services matching the given query against the specified feed.
    /// Paginates automatically and yields results as they arrive.
    /// Client-side filtering is applied for criteria not supported by the endpoint.
    /// </summary>
    /// <param name="feedBaseUrl">
    /// The base URL of the ORUK v3 API, e.g. <c>https://example.org/o/OpenReferralService/v3</c>.
    /// The <c>/services</c> path is appended automatically.
    /// </param>
    /// <param name="query">Typed filter and pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    IAsyncEnumerable<OrukService> SearchAsync(
        Uri feedBaseUrl,
        OrukServiceQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full record for a single service by its ID.
    /// Returns <see langword="null"/> if the service does not exist (404).
    /// </summary>
    /// <param name="feedBaseUrl">The base URL of the ORUK v3 API.</param>
    /// <param name="serviceId">The ORUK service ID (UUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<OrukService?> GetByIdAsync(
        Uri feedBaseUrl,
        string serviceId,
        CancellationToken cancellationToken = default);
}
