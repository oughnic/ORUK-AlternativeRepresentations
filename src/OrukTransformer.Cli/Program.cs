using System.CommandLine;
using Microsoft.Extensions.Logging;
using OrukTransformer.Cli;
using OrukTransformer.Cli.Fetching;
using OrukTransformer.Cli.Output;
using OrukTransformer.Core.Mapping;

// ── Options ──────────────────────────────────────────────────────────────────

var orukUrlOption = new Option<string>("--oruk-url")
{
    Description = "URL of the ORUK v3 GET /services endpoint.",
    Required = true
};

var jsonLdOption = new Option<FileInfo?>("--json-ld")
{
    Description = "File path to write the generated JSON-LD output. " +
                  "Omit to write to stdout.",
    DefaultValueFactory = _ => null
};

var maxRecordsOption = new Option<int>("--max-records")
{
    Description = "Maximum number of service records to retrieve. " +
                  "Values less than 1 mean no limit.",
    DefaultValueFactory = _ => 50
};

var verboseOption = new Option<bool>("--verbose")
{
    Description = "Emit per-service VODIM field-level data-quality detail " +
                  "in addition to the summary report.",
    DefaultValueFactory = _ => false
};

var dataQualityReportOption = new Option<FileInfo?>("--data-quality-report")
{
    Description = "Write an xHTML5 data-quality report to this file. " +
                  "When omitted the report is not generated. " +
                  "Supply a path, e.g. --data-quality-report oruk-schema_org.html",
    DefaultValueFactory = _ => null
};

// ── Root command ──────────────────────────────────────────────────────────────

var rootCommand = new RootCommand(
    "Fetches an ORUK v3 service-directory endpoint, transforms the services " +
    "to Schema.org JSON-LD, and reports VODIM data quality.")
{
    orukUrlOption,
    jsonLdOption,
    maxRecordsOption,
    verboseOption,
    dataQualityReportOption
};

rootCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
{
    var orukUrlRaw = parseResult.GetValue(orukUrlOption)!;
    if (!Uri.TryCreate(orukUrlRaw, UriKind.Absolute, out var orukUrl))
    {
        Console.Error.WriteLine($"Error: '--oruk-url' value '{orukUrlRaw}' is not a valid absolute URI.");
        Environment.Exit(1);
        return;
    }
    var jsonLd = parseResult.GetValue(jsonLdOption);
    var maxRecords = parseResult.GetValue(maxRecordsOption);
    var verbose = parseResult.GetValue(verboseOption);
    var dataQualityReport = parseResult.GetValue(dataQualityReportOption);

    // ── HTTP client ───────────────────────────────────────────────────────────

    using var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
        "OrukTransformer.Cli/1.0 (+https://github.com/iStandUK/ORUK-AlternativeRepresentations)");

    // ── Logging ───────────────────────────────────────────────────────────────

    using var loggerFactory = LoggerFactory.Create(builder =>
        builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

    // ── Services ──────────────────────────────────────────────────────────────

    var fetcher = new OrukFeedPageFetcher(
        httpClient, loggerFactory.CreateLogger<OrukFeedPageFetcher>());

    var transformer = new OrukToSchemaOrgTransformer();
    var merger = new JsonLdMerger();
    var writer = new JsonLdWriter();
    var reporter = new VodimReporter();
    var dataQualityReportWriter = new HtmlDataQualityReportWriter();

    var runner = new RunCommand(
        fetcher,
        transformer,
        merger,
        writer,
        reporter,
        dataQualityReportWriter,
        loggerFactory.CreateLogger<RunCommand>());

    var exitCode = await runner.ExecuteAsync(orukUrl, jsonLd, maxRecords, verbose,
        dataQualityReport, cancellationToken);
    Environment.Exit(exitCode);
});

var result = rootCommand.Parse(args);
return await result.InvokeAsync();
