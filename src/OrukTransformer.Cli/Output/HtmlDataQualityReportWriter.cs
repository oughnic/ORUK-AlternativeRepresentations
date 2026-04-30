using System.Net;
using System.Text;
using OrukTransformer.Core.Vodim;

namespace OrukTransformer.Cli.Output;

/// <summary>
/// Generates an xHTML5 VODIM data-quality report with embedded iStandUK-branded CSS.
/// </summary>
public sealed class HtmlDataQualityReportWriter : IDataQualityReportWriter
{
    /// <inheritdoc/>
    public async Task WriteAsync(
        IReadOnlyList<TransformationReport> reports,
        Uri sourceUrl,
        FileInfo outputFile,
        CancellationToken cancellationToken = default)
    {
        var html = BuildHtml(reports, sourceUrl);
        await File.WriteAllTextAsync(outputFile.FullName, html, Encoding.UTF8, cancellationToken);
    }

    // ── Internal builder (internal for testability) ───────────────────────────────

    internal static string BuildHtml(IReadOnlyList<TransformationReport> reports, Uri sourceUrl)
    {
        // Aggregate all records across all services, grouped by SourcePath
        var allRecords = reports.SelectMany(r => r.Records).ToList();

        var fieldGroups = allRecords
            .GroupBy(r => r.SourcePath, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var generated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        sb.AppendLine($"  <title>ORUK Data Quality Report — {Encode(sourceUrl.ToString())}</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine(EmbeddedCss);
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // ── Header ────────────────────────────────────────────────────────────────
        sb.AppendLine("  <header>");
        sb.AppendLine("    <div class=\"logo-bar\">");
        sb.AppendLine("      <span class=\"logo-text\">iStandUK</span>");
        sb.AppendLine("      <span class=\"logo-sub\">Open Referral UK</span>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </header>");

        sb.AppendLine("  <main>");
        sb.AppendLine("    <h1>ORUK Data Quality Report</h1>");
        sb.AppendLine("    <div class=\"meta\">");
        sb.AppendLine($"      <p><strong>Source:</strong> <a href=\"{Encode(sourceUrl.ToString())}\">{Encode(sourceUrl.ToString())}</a></p>");
        sb.AppendLine($"      <p><strong>Services assessed:</strong> {reports.Count}</p>");
        sb.AppendLine($"      <p><strong>Generated:</strong> {Encode(generated)}</p>");
        sb.AppendLine("    </div>");

        // ── Overall summary ───────────────────────────────────────────────────────
        int totalV = reports.Sum(r => r.ValidCount);
        int totalO = reports.Sum(r => r.OtherCount);
        int totalD = reports.Sum(r => r.DefaultCount);
        int totalI = reports.Sum(r => r.InvalidCount);
        int totalM = reports.Sum(r => r.MissingCount);
        int totalU = reports.Sum(r => r.UnmappedCount);
        int totalAll = totalV + totalO + totalD + totalI + totalM + totalU;

        sb.AppendLine("    <section class=\"summary\">");
        sb.AppendLine("      <h2>Overall VODIM Summary</h2>");
        sb.AppendLine("      <table class=\"vodim-table summary-table\">");
        sb.AppendLine("        <thead><tr>");
        sb.AppendLine("          <th>Code</th><th>Classification</th><th>Count</th><th>Percentage</th>");
        sb.AppendLine("        </tr></thead>");
        sb.AppendLine("        <tbody>");
        AppendVodimRow(sb, "V", "Valid", totalV, totalAll, "valid");
        AppendVodimRow(sb, "O", "Other", totalO, totalAll, "other");
        AppendVodimRow(sb, "D", "Default", totalD, totalAll, "default");
        AppendVodimRow(sb, "I", "Invalid", totalI, totalAll, "invalid");
        AppendVodimRow(sb, "M", "Missing", totalM, totalAll, "missing");
        AppendVodimRow(sb, "U", "Unmapped", totalU, totalAll, "unmapped");
        sb.AppendLine("        </tbody>");
        sb.AppendLine("        <tfoot><tr>");
        sb.AppendLine($"          <td colspan=\"2\"><strong>Total fields</strong></td>");
        sb.AppendLine($"          <td><strong>{totalAll}</strong></td><td></td>");
        sb.AppendLine("        </tr></tfoot>");
        sb.AppendLine("      </table>");
        sb.AppendLine("    </section>");

        // ── Per-field sections ────────────────────────────────────────────────────
        sb.AppendLine("    <section class=\"fields\">");
        sb.AppendLine("      <h2 class=\"section-heading\">Field-by-Field Breakdown</h2>");

        if (fieldGroups.Count == 0)
        {
            sb.AppendLine("      <p class=\"no-data\">No fields recorded.</p>");
        }
        else
        {
            foreach (var group in fieldGroups)
            {
                var records = group.ToList();
                int fV = records.Count(r => r.Classification == VodimClassification.Valid);
                int fO = records.Count(r => r.Classification == VodimClassification.Other);
                int fD = records.Count(r => r.Classification == VodimClassification.Default);
                int fI = records.Count(r => r.Classification == VodimClassification.Invalid);
                int fM = records.Count(r => r.Classification == VodimClassification.Missing);
                int fU = records.Count(r => r.Classification == VodimClassification.Unmapped);
                int fTotal = records.Count;

                // Collect target path (use the most common non-dash value)
                var targetPath = records
                    .Select(r => r.TargetPath)
                    .Where(p => p != "—")
                    .GroupBy(p => p)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "—";

                // Anchor id based on field path
                var anchorId = "field-" + Encode(group.Key.Replace('.', '-').Replace('[', '-').Replace(']', '-'));

                sb.AppendLine($"      <section class=\"field\" id=\"{anchorId}\">");
                sb.AppendLine($"        <h2 class=\"field-heading\">{Encode(group.Key)}</h2>");
                sb.AppendLine($"        <p class=\"target-path\">→ <code>{Encode(targetPath)}</code></p>");

                sb.AppendLine("        <table class=\"vodim-table field-table\">");
                sb.AppendLine("          <thead><tr>");
                sb.AppendLine("            <th>Code</th><th>Classification</th><th>Count</th><th>Percentage</th>");
                sb.AppendLine("          </tr></thead>");
                sb.AppendLine("          <tbody>");
                AppendVodimRow(sb, "V", "Valid", fV, fTotal, "valid", indent: 12);
                AppendVodimRow(sb, "O", "Other", fO, fTotal, "other", indent: 12);
                AppendVodimRow(sb, "D", "Default", fD, fTotal, "default", indent: 12);
                AppendVodimRow(sb, "I", "Invalid", fI, fTotal, "invalid", indent: 12);
                AppendVodimRow(sb, "M", "Missing", fM, fTotal, "missing", indent: 12);
                AppendVodimRow(sb, "U", "Unmapped", fU, fTotal, "unmapped", indent: 12);
                sb.AppendLine("          </tbody>");
                sb.AppendLine("        </table>");

                // Aggregate notes (without instance-specific source values)
                var notes = records
                    .Where(r => r.Note is not null)
                    .GroupBy(r => r.Note!, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                if (notes.Count > 0)
                {
                    sb.AppendLine("        <div class=\"notes\">");
                    sb.AppendLine("          <h4>Issue Notes</h4>");
                    sb.AppendLine("          <table class=\"notes-table\">");
                    sb.AppendLine("            <thead><tr><th>Note</th><th>Occurrences</th></tr></thead>");
                    sb.AppendLine("            <tbody>");
                    foreach (var noteGroup in notes)
                    {
                        var classification = records
                            .First(r => string.Equals(r.Note, noteGroup.Key, StringComparison.OrdinalIgnoreCase))
                            .Classification;
                        sb.AppendLine(
                            $"              <tr class=\"{ClassificationCssClass(classification)}\">" +
                            $"<td>{Encode(noteGroup.Key)}</td>" +
                            $"<td class=\"count\">{noteGroup.Count()}</td></tr>");
                    }
                    sb.AppendLine("            </tbody>");
                    sb.AppendLine("          </table>");
                    sb.AppendLine("        </div>");
                }

                sb.AppendLine("      </section>");
            }
        }

        sb.AppendLine("    </section>");
        sb.AppendLine("  </main>");

        sb.AppendLine("  <footer>");
        sb.AppendLine("    <p>Generated by <strong>OrukTransformer</strong> — " +
                      "<a href=\"https://github.com/iStandUK/ORUK-AlternativeRepresentations\">" +
                      "iStandUK/ORUK-AlternativeRepresentations</a></p>");
        sb.AppendLine("  </footer>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private static void AppendVodimRow(
        StringBuilder sb, string code, string label,
        int count, int total, string cssClass, int indent = 10)
    {
        var pct = total == 0 ? "0%" : $"{count * 100 / total}%";
        var pad = new string(' ', indent);
        sb.AppendLine(
            $"{pad}<tr class=\"vodim-{cssClass}\">" +
            $"<td class=\"code\">{Encode(code)}</td>" +
            $"<td>{Encode(label)}</td>" +
            $"<td class=\"count\">{count}</td>" +
            $"<td class=\"pct\">{pct}</td></tr>");
    }

    private static string ClassificationCssClass(VodimClassification c) => c switch
    {
        VodimClassification.Valid => "vodim-valid",
        VodimClassification.Other => "vodim-other",
        VodimClassification.Default => "vodim-default",
        VodimClassification.Invalid => "vodim-invalid",
        VodimClassification.Missing => "vodim-missing",
        VodimClassification.Unmapped => "vodim-unmapped",
        _ => string.Empty
    };

    private static string Encode(string s) => WebUtility.HtmlEncode(s);

    // ── Embedded CSS (iStandUK branding) ─────────────────────────────────────────

    private const string EmbeddedCss = """
        /* iStandUK brand colours */
        :root {
          --istanduk-blue:       #1a3f6f;
          --istanduk-blue-mid:   #2a5fa8;
          --istanduk-teal:       #007b84;
          --istanduk-teal-light: #e6f4f5;
          --istanduk-white:      #ffffff;
          --istanduk-off-white:  #f5f7fa;
          --istanduk-border:     #d0d9e6;
          --istanduk-text:       #1a1a2e;
          --istanduk-text-light: #4a5568;

          /* VODIM classification colours */
          --vodim-valid:   #1a7a3c;
          --vodim-other:   #7a5c00;
          --vodim-default: #1a5c7a;
          --vodim-invalid: #9a1a1a;
          --vodim-missing: #4a4a4a;
          --vodim-unmapped:#5a3a7a;

          --vodim-valid-bg:   #e8f5ec;
          --vodim-other-bg:   #fef9e6;
          --vodim-default-bg: #e6f2fa;
          --vodim-invalid-bg: #fce8e8;
          --vodim-missing-bg: #f0f0f0;
          --vodim-unmapped-bg:#f0ebfa;
        }

        *, *::before, *::after { box-sizing: border-box; }

        body {
          font-family: 'Segoe UI', Arial, sans-serif;
          font-size: 16px;
          line-height: 1.6;
          color: var(--istanduk-text);
          background: var(--istanduk-off-white);
          margin: 0;
          padding: 0;
        }

        /* ── Header ──────────────────────────────────────────────── */
        header {
          background: var(--istanduk-blue);
          color: var(--istanduk-white);
          padding: 1rem 2rem;
          border-bottom: 4px solid var(--istanduk-teal);
        }

        .logo-bar {
          display: flex;
          align-items: baseline;
          gap: 1rem;
        }

        .logo-text {
          font-size: 1.6rem;
          font-weight: 700;
          letter-spacing: 0.05em;
        }

        .logo-sub {
          font-size: 1rem;
          opacity: 0.85;
        }

        /* ── Main content ────────────────────────────────────────── */
        main {
          max-width: 1100px;
          margin: 2rem auto;
          padding: 0 1.5rem;
        }

        h1 {
          font-size: 1.9rem;
          color: var(--istanduk-blue);
          border-bottom: 3px solid var(--istanduk-teal);
          padding-bottom: 0.4rem;
          margin-bottom: 1.2rem;
        }

        h2 {
          font-size: 1.4rem;
          color: var(--istanduk-blue-mid);
          margin-top: 2.5rem;
          margin-bottom: 0.8rem;
          padding-left: 0.5rem;
          border-left: 4px solid var(--istanduk-teal);
        }

        /* Section-level h2 (Overall Summary, Field-by-Field Breakdown) */
        h2.section-heading {
          font-size: 1.4rem;
        }

        /* Field-path h2 headings — monospace to match code paths */
        h2.field-heading {
          font-family: 'Courier New', Courier, monospace;
          font-size: 1.05rem;
          background: var(--istanduk-teal-light);
          padding: 0.4rem 0.8rem;
          border-radius: 4px;
          border-left: 4px solid var(--istanduk-teal);
          margin: 0;
        }

        h4 {
          font-size: 0.95rem;
          color: var(--istanduk-text-light);
          margin: 0.8rem 0 0.3rem;
          text-transform: uppercase;
          letter-spacing: 0.04em;
        }

        /* ── Meta block ──────────────────────────────────────────── */
        .meta {
          background: var(--istanduk-white);
          border: 1px solid var(--istanduk-border);
          border-radius: 6px;
          padding: 1rem 1.5rem;
          margin-bottom: 2rem;
        }

        .meta p { margin: 0.3rem 0; }

        .meta a {
          color: var(--istanduk-teal);
          word-break: break-all;
        }

        /* ── Tables ──────────────────────────────────────────────── */
        .vodim-table, .notes-table {
          width: 100%;
          border-collapse: collapse;
          background: var(--istanduk-white);
          border: 1px solid var(--istanduk-border);
          border-radius: 6px;
          overflow: hidden;
          margin-bottom: 0.8rem;
          font-size: 0.92rem;
        }

        .vodim-table th, .notes-table th {
          background: var(--istanduk-blue);
          color: var(--istanduk-white);
          text-align: left;
          padding: 0.5rem 0.8rem;
          font-weight: 600;
        }

        .vodim-table td, .notes-table td {
          padding: 0.4rem 0.8rem;
          border-top: 1px solid var(--istanduk-border);
        }

        .vodim-table tfoot td {
          background: var(--istanduk-off-white);
          border-top: 2px solid var(--istanduk-border);
        }

        td.code {
          font-family: 'Courier New', Courier, monospace;
          font-weight: 700;
          width: 2.5em;
        }

        td.count, td.pct { text-align: right; width: 6em; }

        /* VODIM row colours */
        tr.vodim-valid   { color: var(--vodim-valid);   background: var(--vodim-valid-bg); }
        tr.vodim-other   { color: var(--vodim-other);   background: var(--vodim-other-bg); }
        tr.vodim-default { color: var(--vodim-default); background: var(--vodim-default-bg); }
        tr.vodim-invalid { color: var(--vodim-invalid); background: var(--vodim-invalid-bg); }
        tr.vodim-missing { color: var(--vodim-missing); background: var(--vodim-missing-bg); }
        tr.vodim-unmapped{ color: var(--vodim-unmapped);background: var(--vodim-unmapped-bg); }

        /* ── Summary section ──────────────────────────────────────── */
        .summary { margin-bottom: 2.5rem; }

        .summary-table { max-width: 500px; }

        /* ── Field sections ───────────────────────────────────────── */
        .field {
          background: var(--istanduk-white);
          border: 1px solid var(--istanduk-border);
          border-radius: 6px;
          padding: 1rem 1.5rem;
          margin-bottom: 1.5rem;
        }

        .field h2.field-heading { margin-top: 0; }

        .field-table { max-width: 500px; }

        .target-path {
          font-size: 0.88rem;
          color: var(--istanduk-text-light);
          margin: 0 0 0.8rem;
        }

        .target-path code {
          font-family: 'Courier New', Courier, monospace;
          background: var(--istanduk-off-white);
          padding: 0.1em 0.4em;
          border-radius: 3px;
        }

        .notes { margin-top: 0.8rem; }

        .no-data {
          color: var(--istanduk-text-light);
          font-style: italic;
        }

        /* ── Footer ──────────────────────────────────────────────── */
        footer {
          background: var(--istanduk-blue);
          color: rgba(255,255,255,0.75);
          text-align: center;
          padding: 1rem;
          margin-top: 3rem;
          font-size: 0.85rem;
        }

        footer a { color: var(--istanduk-teal-light); }
        """;
}
