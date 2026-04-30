using OrukModels.Models;

namespace OrukTransformer.Cli.Fetching;

/// <summary>
/// Fetches ORUK service records from a paged ORUK v3 <c>/services</c> endpoint.
/// </summary>
public interface IOrukFeedPageFetcher
{
    /// <summary>
    /// Asynchronously yields <see cref="OrukService"/> records from the endpoint,
    /// fetching pages on demand until <paramref name="maxRecords"/> has been
    /// reached (or all pages have been consumed when <paramref name="maxRecords"/>
    /// is less than 1).
    /// </summary>
    /// <param name="endpointUrl">The base URL of the ORUK <c>GET /services</c> endpoint.</param>
    /// <param name="maxRecords">
    /// Maximum number of service records to return.  A value less than 1 means
    /// no limit — all available records are returned.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel long-running fetches.</param>
    IAsyncEnumerable<OrukService> FetchAsync(
        Uri endpointUrl,
        int maxRecords,
        CancellationToken cancellationToken = default);
}
