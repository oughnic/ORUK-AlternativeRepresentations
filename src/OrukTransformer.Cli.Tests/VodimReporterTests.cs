using OrukTransformer.Cli.Output;
using OrukTransformer.Core.Vodim;

namespace OrukTransformer.Cli.Tests;

/// <summary>
/// Tests for <see cref="VodimReporter.BuildSummaryText"/>.
///
/// Tests drive the internal static helper directly (via <c>internal</c> visibility)
/// so that summary formatting can be verified without needing to capture
/// Console.Out / Console.Error streams.
/// </summary>
public class VodimReporterTests
{
    private static readonly Uri SourceUrl = new("https://example.org/services");

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static TransformationReport BuildReport(
        string serviceId,
        int valid = 0, int other = 0, int defaults = 0,
        int invalid = 0, int missing = 0, int unmapped = 0)
    {
        var report = new TransformationReport(serviceId);
        AddRecords(report, VodimClassification.Valid, valid);
        AddRecords(report, VodimClassification.Other, other);
        AddRecords(report, VodimClassification.Default, defaults);
        AddRecords(report, VodimClassification.Invalid, invalid);
        AddRecords(report, VodimClassification.Missing, missing);
        AddRecords(report, VodimClassification.Unmapped, unmapped);
        return report;
    }

    private static void AddRecords(
        TransformationReport report,
        VodimClassification classification,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Use reflection to call the internal Add method
            var method = typeof(TransformationReport)
                .GetMethod("Add", System.Reflection.BindingFlags.NonPublic
                                | System.Reflection.BindingFlags.Instance)!;
            method.Invoke(report, [new FieldMappingRecord
            {
                SourcePath = $"service.field{i}",
                TargetPath = $"GovernmentService.prop{i}",
                Classification = classification
            }]);
        }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void BuildSummaryText_EmptyReports_ShowsZeroTotals()
    {
        var text = VodimReporter.BuildSummaryText([], SourceUrl);

        Assert.Contains("0 service(s)", text);
        Assert.Contains("Total fields:", text);
        Assert.Contains("     0", text); // all counts are zero
    }

    [Fact]
    public void BuildSummaryText_SingleReport_ShowsCorrectCounts()
    {
        var report = BuildReport("svc-1", valid: 10, other: 2, defaults: 1, invalid: 0, missing: 5);
        var text = VodimReporter.BuildSummaryText([report], SourceUrl);

        Assert.Contains("1 service(s)", text);
        Assert.Contains("V Valid", text);
        Assert.Contains("O Other", text);
        Assert.Contains("D Default", text);
        Assert.Contains("I Invalid", text);
        Assert.Contains("M Missing", text);
        Assert.Contains("U Unmapped", text);
        Assert.Contains("Total fields:", text);
    }

    [Fact]
    public void BuildSummaryText_UnmappedFields_AppearsInSummary()
    {
        var report = BuildReport("svc-1", valid: 5, unmapped: 3);
        var text = VodimReporter.BuildSummaryText([report], SourceUrl);

        Assert.Contains("U Unmapped", text);
        Assert.Contains("     3", text);
    }

    [Fact]
    public void BuildSummaryText_MultipleReports_AggregatesTotals()
    {
        var r1 = BuildReport("svc-1", valid: 10, missing: 2);
        var r2 = BuildReport("svc-2", valid: 5, invalid: 1);

        var text = VodimReporter.BuildSummaryText([r1, r2], SourceUrl);

        Assert.Contains("2 service(s)", text);
        // V Valid = 15, I Invalid = 1, M Missing = 2, Total = 18
        Assert.Contains("    15", text);
        Assert.Contains("     1", text); // invalid
        Assert.Contains("     2", text); // missing
    }

    [Fact]
    public void BuildSummaryText_IncludesSourceUrl()
    {
        var text = VodimReporter.BuildSummaryText([], SourceUrl);

        Assert.Contains(SourceUrl.ToString(), text);
    }

    [Fact]
    public void BuildSummaryText_ShowsPercentages()
    {
        // 100 valid, 0 everything else → 100%
        var report = BuildReport("svc-1", valid: 100);
        var text = VodimReporter.BuildSummaryText([report], SourceUrl);

        Assert.Contains("100%", text);
    }

    [Fact]
    public void BuildSummaryText_TotalFieldsIsSum()
    {
        var report = BuildReport("svc-1", valid: 3, other: 2, defaults: 1, invalid: 1, missing: 2);
        var text = VodimReporter.BuildSummaryText([report], SourceUrl);

        // Total = 9
        Assert.Contains("     9", text);
    }
}
