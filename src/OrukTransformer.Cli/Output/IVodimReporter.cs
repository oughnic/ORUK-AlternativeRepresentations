using OrukTransformer.Core.Vodim;

namespace OrukTransformer.Cli.Output;

/// <summary>
/// Writes VODIM data-quality reports for a batch of transformed ORUK services.
/// </summary>
public interface IVodimReporter
{
    /// <summary>
    /// Writes a VODIM quality report for the given set of
    /// <see cref="TransformationReport"/> objects.
    ///
    /// <para>
    /// In normal mode, a single summary table is emitted.
    /// When <paramref name="verbose"/> is <c>true</c>, per-service detail is
    /// additionally written for every service that has <c>Other</c> or
    /// <c>Invalid</c> records.
    /// </para>
    ///
    /// <para>
    /// The report is written to <see cref="Console.Error"/> when JSON-LD is being
    /// written to stdout (i.e. <paramref name="jsonLdToFile"/> is <c>false</c>),
    /// and to <see cref="Console.Out"/> otherwise.
    /// </para>
    /// </summary>
    /// <param name="reports">Per-service transformation reports.</param>
    /// <param name="sourceUrl">The ORUK endpoint URL, used in the summary header.</param>
    /// <param name="verbose">
    /// When <c>true</c>, emit per-service field-level detail after the summary.
    /// </param>
    /// <param name="jsonLdToFile">
    /// When <c>true</c>, JSON-LD is being written to a file, so the report
    /// may safely go to stdout.
    /// </param>
    void WriteReport(
        IReadOnlyList<TransformationReport> reports,
        Uri sourceUrl,
        bool verbose,
        bool jsonLdToFile);
}
