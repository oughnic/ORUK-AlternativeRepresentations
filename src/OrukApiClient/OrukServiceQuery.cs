namespace OrukApiClient;

/// <summary>
/// Encapsulates the parameters for a filtered query against an ORUK v3 /services endpoint.
/// All filter properties are optional; omitted properties are not applied.
/// </summary>
public record OrukServiceQuery
{
    /// <summary>Free-text keyword search term.</summary>
    public string? Keyword { get; init; }

    /// <summary>
    /// Taxonomy term IDs (UUIDs as returned by <see cref="IOrukTaxonomyClient"/>)
    /// to filter by. Multiple terms are OR-combined. Applied client-side if the
    /// endpoint does not support server-side taxonomy filtering.
    /// </summary>
    public IReadOnlyList<string> TaxonomyTermIds { get; init; } = [];

    /// <summary>UK postcode or place name for proximity filtering.</summary>
    public string? Proximity { get; init; }

    /// <summary>Search radius in kilometres. Only applied when <see cref="Proximity"/> is set.</summary>
    public double? RadiusKm { get; init; }

    /// <summary>Minimum age of intended recipients (inclusive).</summary>
    public double? MinimumAge { get; init; }

    /// <summary>Maximum age of intended recipients (inclusive).</summary>
    public double? MaximumAge { get; init; }

    /// <summary>When true, restricts results to services with no cost.</summary>
    public bool FreeOnly { get; init; }

    /// <summary>
    /// Maximum number of service records to return across all pages.
    /// A value less than 1 means no limit — all available records are returned.
    /// Default is 20.
    /// </summary>
    public int MaxRecords { get; init; } = 20;
}
