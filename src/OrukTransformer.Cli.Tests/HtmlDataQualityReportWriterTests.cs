using OrukTransformer.Cli.Output;
using OrukTransformer.Core.Vodim;

namespace OrukTransformer.Cli.Tests;

/// <summary>
/// Tests for <see cref="HtmlDataQualityReportWriter.BuildHtml"/>.
///
/// Tests drive the internal static builder directly so that HTML structure
/// can be verified without writing to disk.
/// </summary>
public class HtmlDataQualityReportWriterTests
{
    private static readonly Uri SourceUrl = new("https://example.org/services");

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static TransformationReport BuildReport(
        string serviceId,
        IEnumerable<FieldMappingRecord> records)
    {
        var report = new TransformationReport(serviceId);
        var addMethod = typeof(TransformationReport)
            .GetMethod("Add", System.Reflection.BindingFlags.NonPublic
                             | System.Reflection.BindingFlags.Instance)!;
        foreach (var rec in records)
            addMethod.Invoke(report, [rec]);
        return report;
    }

    private static FieldMappingRecord MakeRecord(
        string sourcePath,
        string targetPath,
        VodimClassification classification,
        string? sourceValue = null,
        string? note = null) =>
        new()
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            Classification = classification,
            SourceValue = sourceValue,
            Note = note
        };

    // ── Tests ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void BuildHtml_ProducesValidXhtmlDoctype()
    {
        var html = HtmlDataQualityReportWriter.BuildHtml([], SourceUrl);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html xmlns=\"http://www.w3.org/1999/xhtml\"", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</html>", html);
    }

    [Fact]
    public void BuildHtml_EmbedsCss()
    {
        var html = HtmlDataQualityReportWriter.BuildHtml([], SourceUrl);

        Assert.Contains("<style>", html);
        Assert.Contains("--istanduk-blue", html);
        Assert.Contains("</style>", html);
    }

    [Fact]
    public void BuildHtml_IncludesSourceUrl()
    {
        var html = HtmlDataQualityReportWriter.BuildHtml([], SourceUrl);

        Assert.Contains(SourceUrl.ToString(), html);
    }

    [Fact]
    public void BuildHtml_IncludesServiceCount()
    {
        var report = BuildReport("svc-1", [
            MakeRecord("service.name", "GovernmentService.name", VodimClassification.Valid)
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.Contains("1", html); // services assessed
    }

    [Fact]
    public void BuildHtml_EmptyReports_ShowsNoFieldsMessage()
    {
        var html = HtmlDataQualityReportWriter.BuildHtml([], SourceUrl);

        Assert.Contains("No fields recorded", html);
    }

    [Fact]
    public void BuildHtml_FieldPath_AppearsAsHeading()
    {
        var report = BuildReport("svc-1", [
            MakeRecord("service.name", "GovernmentService.name", VodimClassification.Valid)
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.Contains("service.name", html);
        Assert.Contains("<h2 class=\"field-heading\">", html);
    }

    [Fact]
    public void BuildHtml_CountsAggregatedAcrossServices()
    {
        // Two services, each with an invalid record for the same field
        var report1 = BuildReport("svc-1", [
            MakeRecord("service.url", "GovernmentService.url", VodimClassification.Invalid,
                sourceValue: "not-a-url", note: "Not a valid URL")
        ]);
        var report2 = BuildReport("svc-2", [
            MakeRecord("service.url", "GovernmentService.url", VodimClassification.Invalid,
                sourceValue: "also-bad", note: "Not a valid URL")
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report1, report2], SourceUrl);

        // The note should appear once (deduplicated)
        var firstOccurrence = html.IndexOf("Not a valid URL", StringComparison.Ordinal);
        var lastOccurrence = html.LastIndexOf("Not a valid URL", StringComparison.Ordinal);
        Assert.Equal(firstOccurrence, lastOccurrence);

        // The occurrence count (2) should appear somewhere near the note
        Assert.Contains(">2<", html);
    }

    [Fact]
    public void BuildHtml_SourceValueOmittedFromReport()
    {
        const string sensitiveValue = "https://www.second-step.co.uk/";
        var report = BuildReport("svc-1", [
            MakeRecord("organization.website", "Organization.url",
                VodimClassification.Other,
                sourceValue: sensitiveValue,
                note: "Non-standard 'website' field used as fallback for 'url'")
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.DoesNotContain(sensitiveValue, html);
        Assert.Contains("Non-standard &#39;website&#39; field used as fallback for &#39;url&#39;", html);
    }

    [Fact]
    public void BuildHtml_VodimClassificationCodesPresent()
    {
        var report = BuildReport("svc-1", [
            MakeRecord("service.name", "GovernmentService.name", VodimClassification.Valid),
            MakeRecord("service.status", "GovernmentService.additionalProperty",
                VodimClassification.Other, note: "Unrecognised status"),
            MakeRecord("service.fees", "GovernmentService.offers",
                VodimClassification.Default, note: "Defaulted to free"),
            MakeRecord("service.url", "GovernmentService.url",
                VodimClassification.Invalid, note: "Not a URL"),
            MakeRecord("service.description", "GovernmentService.description",
                VodimClassification.Missing),
            MakeRecord("service.extra", "—",
                VodimClassification.Unmapped)
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.Contains(">V<", html);
        Assert.Contains(">O<", html);
        Assert.Contains(">D<", html);
        Assert.Contains(">I<", html);
        Assert.Contains(">M<", html);
        Assert.Contains(">U<", html);
    }

    [Fact]
    public void BuildHtml_MultipleDistinctFieldPaths_EachHasSection()
    {
        var report = BuildReport("svc-1", [
            MakeRecord("service.name", "GovernmentService.name", VodimClassification.Valid),
            MakeRecord("service.url", "GovernmentService.url", VodimClassification.Invalid)
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.Contains("service.name", html);
        Assert.Contains("service.url", html);
        // Two field headings
        Assert.Equal(2, CountOccurrences(html, "field-service-"));
    }

    [Fact]
    public void BuildHtml_NoteWithoutSourceValue_RenderedCleanly()
    {
        var report = BuildReport("svc-1", [
            MakeRecord("service.status", "GovernmentService.additionalProperty",
                VodimClassification.Other,
                sourceValue: null,
                note: "Unrecognised ORUK status")
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.Contains("Unrecognised ORUK status", html);
    }

    [Fact]
    public void BuildHtml_NoNotes_NotesSectionAbsent()
    {
        var report = BuildReport("svc-1", [
            MakeRecord("service.name", "GovernmentService.name", VodimClassification.Valid)
        ]);

        var html = HtmlDataQualityReportWriter.BuildHtml([report], SourceUrl);

        Assert.DoesNotContain("Issue Notes", html);
    }

    // ── Utility ───────────────────────────────────────────────────────────────────

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0, index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
