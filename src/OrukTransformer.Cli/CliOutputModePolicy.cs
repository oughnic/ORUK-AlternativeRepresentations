using Microsoft.Extensions.Logging;

namespace OrukTransformer.Cli;

internal static class CliOutputModePolicy
{
    internal static string? ValidateForOutputMode(
        bool writingJsonToStdout,
        bool verboseEnabled,
        bool logLevelProvided,
        bool quietProvided)
    {
        if (!writingJsonToStdout)
            return null;

        if (verboseEnabled)
            return "Option '--verbose' cannot be used when '--json-ld' is omitted.";

        if (logLevelProvided || quietProvided)
            return "Options '--log-level' and '--quiet' cannot be used when '--json-ld' is omitted.";

        return null;
    }

    internal static LogLevel ResolveEffectiveLogLevel(
        bool writingJsonToStdout,
        bool quiet,
        string logLevelRaw)
    {
        if (writingJsonToStdout)
            return LogLevel.None;

        if (quiet)
            return LogLevel.Warning;

        return ParseLogLevel(logLevelRaw);
    }

    internal static LogLevel ParseLogLevel(string value) =>
        value.ToLowerInvariant() switch
        {
            "trace" => LogLevel.Trace,
            "debug" => LogLevel.Debug,
            "information" => LogLevel.Information,
            "warning" => LogLevel.Warning,
            "error" => LogLevel.Error,
            "critical" => LogLevel.Critical,
            "none" => LogLevel.None,
            _ => LogLevel.Information
        };
}
