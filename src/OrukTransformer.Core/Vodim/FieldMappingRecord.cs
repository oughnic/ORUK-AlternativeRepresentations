namespace OrukTransformer.Core.Vodim;

/// <summary>
/// An immutable record of how a single ORUK source field was classified and
/// (optionally) mapped to a Schema.org target property.
///
/// One <see cref="FieldMappingRecord"/> is produced for every ORUK field that
/// <em>could potentially be mapped</em>, regardless of whether mapping succeeded.
/// Fields that have no Schema.org mapping have <see cref="TargetPath"/> set to
/// <c>"—"</c> and a <see cref="Classification"/> of <see cref="VodimClassification.Missing"/>.
/// </summary>
public record FieldMappingRecord
{
    /// <summary>
    /// Dot-separated path to the ORUK source field
    /// (e.g. <c>"service.name"</c>, <c>"service.schedules[0].opens_at"</c>).
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// Dot-separated path to the Schema.org target property
    /// (e.g. <c>"GovernmentService.name"</c>, <c>"OpeningHoursSpecification.opens"</c>).
    /// Use <c>"—"</c> when no Schema.org mapping exists for this field.
    /// </summary>
    public required string TargetPath { get; init; }

    /// <summary>VODIM quality classification for this field.</summary>
    public required VodimClassification Classification { get; init; }

    /// <summary>
    /// String representation of the raw source value, truncated to 200 characters.
    /// <c>null</c> when the source field was absent.
    /// </summary>
    public string? SourceValue { get; init; }

    /// <summary>
    /// String representation of the mapped (output) value, truncated to 200 characters.
    /// <c>null</c> when the field was not mapped (Missing or Invalid).
    /// </summary>
    public string? MappedValue { get; init; }

    /// <summary>
    /// Optional explanatory note — used for Default substitutions, Other vocabulary
    /// mismatches, and Invalid format failures.
    /// </summary>
    public string? Note { get; init; }

    /// <summary>Returns a concise single-line summary for logging.</summary>
    public override string ToString() =>
        $"[{Classification.ToString()[0]}] {SourcePath} → {TargetPath}" +
        (SourceValue is not null ? $" (source: \"{Truncate(SourceValue, 60)}\")" : string.Empty) +
        (Note is not null ? $" — {Note}" : string.Empty);

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
