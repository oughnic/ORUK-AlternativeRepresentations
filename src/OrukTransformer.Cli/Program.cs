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

var logLevelOption = new Option<string>("--log-level")
{
    Description = "Minimum log level for console output. " +
                  "Valid values: trace, debug, information, warning, error, critical, none. " +
                  "Defaults to 'information'.",
    DefaultValueFactory = _ => "information"
};

var quietOption = new Option<bool>("--quiet")
{
    Description = "Suppress informational log output (equivalent to --log-level warning). " +
                  "VODIM summary is always written regardless of this flag.",
    DefaultValueFactory = _ => false
};

var timeoutOption = new Option<int>("--timeout")
{
    Description = "HTTP request timeout in seconds for each page fetch. Defaults to 30.",
    DefaultValueFactory = _ => 30
};

var formatOption = new Option<string>("--format")
{
    Description = "Output format. Currently only 'json-ld' is supported. " +
                  "Defaults to 'json-ld'.",
    DefaultValueFactory = _ => "json-ld"
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
    dataQualityReportOption,
    logLevelOption,
    quietOption,
    timeoutOption,
    formatOption
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
    var logLevelRaw = parseResult.GetValue(logLevelOption)!;
    var quiet = parseResult.GetValue(quietOption);
    var timeoutSeconds = parseResult.GetValue(timeoutOption);
    var format = parseResult.GetValue(formatOption)!;

    // Validate --format
    if (!string.Equals(format, "json-ld", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine(
            $"Error: '--format' value '{format}' is not supported. Only 'json-ld' is currently available.");
        Environment.Exit(1);
        return;
    }

    // ── Resolve log level ─────────────────────────────────────────────────────

    var logLevel = quiet
        ? LogLevel.Warning
        : ParseLogLevel(logLevelRaw);

    // ── HTTP client ───────────────────────────────────────────────────────────

    using var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : 30)
    };
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
        "OrukTransformer.Cli/1.0 (+https://github.com/iStandUK/ORUK-AlternativeRepresentations)");

    // ── Logging ───────────────────────────────────────────────────────────────

    using var loggerFactory = LoggerFactory.Create(builder =>
        builder.AddConsole().SetMinimumLevel(logLevel));

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

// ── Helpers ───────────────────────────────────────────────────────────────────

static LogLevel ParseLogLevel(string value) =>
    value.ToLowerInvariant() switch
    {
        "trace"       => LogLevel.Trace,
        "debug"       => LogLevel.Debug,
        "information" => LogLevel.Information,
        "warning"     => LogLevel.Warning,
        "error"       => LogLevel.Error,
        "critical"    => LogLevel.Critical,
        "none"        => LogLevel.None,
        _             => LogLevel.Information
    };

