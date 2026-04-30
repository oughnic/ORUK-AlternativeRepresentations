namespace OrukTransformer.Core.Vodim;

/// <summary>
/// Accumulates <see cref="FieldMappingRecord"/> entries during a single
/// ORUK → Schema.org transformation and exposes per-classification summaries.
///
/// One report is produced per <c>OrukService</c> transformation.  It covers all
/// entity types that were mapped as part of that service (organisation, locations,
/// schedules, contacts, offers, etc.).
///
/// The report is mutable during construction and effectively frozen once returned
/// inside a <c>TransformationResult</c>.
/// </summary>
public sealed class TransformationReport
{
    private readonly List<FieldMappingRecord> _records = [];

    /// <summary>
    /// The ORUK service identifier for which this report was produced.
    /// </summary>
    public string ServiceId { get; }

    /// <summary>All field mapping records accumulated so far.</summary>
    public IReadOnlyList<FieldMappingRecord> Records => _records;

    /// <summary>Initialises a new report for the given service identifier.</summary>
    public TransformationReport(string serviceId)
    {
        ServiceId = serviceId;
    }

    /// <summary>Appends a field mapping record to the report.</summary>
    internal void Add(FieldMappingRecord record) => _records.Add(record);

    // ── Per-classification counts ────────────────────────────────────────────────

    /// <summary>Number of fields classified as <see cref="VodimClassification.Valid"/>.</summary>
    public int ValidCount => CountOf(VodimClassification.Valid);

    /// <summary>Number of fields classified as <see cref="VodimClassification.Other"/>.</summary>
    public int OtherCount => CountOf(VodimClassification.Other);

    /// <summary>Number of fields classified as <see cref="VodimClassification.Default"/>.</summary>
    public int DefaultCount => CountOf(VodimClassification.Default);

    /// <summary>Number of fields classified as <see cref="VodimClassification.Invalid"/>.</summary>
    public int InvalidCount => CountOf(VodimClassification.Invalid);

    /// <summary>Number of fields classified as <see cref="VodimClassification.Missing"/>.</summary>
    public int MissingCount => CountOf(VodimClassification.Missing);

    /// <summary>Total number of field mapping records in this report.</summary>
    public int TotalCount => _records.Count;

    /// <summary>
    /// Returns all records with the given classification.
    /// </summary>
    public IReadOnlyList<FieldMappingRecord> ByClassification(VodimClassification classification) =>
        _records.Where(r => r.Classification == classification).ToList();

    /// <summary>
    /// Returns all records for a given source-path prefix
    /// (e.g. <c>"service"</c> to get all service-level records).
    /// </summary>
    public IReadOnlyList<FieldMappingRecord> BySourcePrefix(string prefix) =>
        _records.Where(r => r.SourcePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// Produces a concise multi-line summary string suitable for logging.
    /// </summary>
    public string Summary()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Transformation report for service {ServiceId}");
        sb.AppendLine($"  Total fields assessed : {TotalCount}");
        sb.AppendLine($"  V Valid               : {ValidCount}");
        sb.AppendLine($"  O Other               : {OtherCount}");
        sb.AppendLine($"  D Default             : {DefaultCount}");
        sb.AppendLine($"  I Invalid             : {InvalidCount}");
        sb.AppendLine($"  M Missing             : {MissingCount}");
        if (OtherCount > 0 || InvalidCount > 0)
        {
            sb.AppendLine("  Issues:");
            foreach (var r in _records.Where(
                r => r.Classification is VodimClassification.Other or VodimClassification.Invalid))
            {
                sb.AppendLine($"    {r}");
            }
        }
        return sb.ToString().TrimEnd();
    }

    private int CountOf(VodimClassification c) => _records.Count(r => r.Classification == c);
}
