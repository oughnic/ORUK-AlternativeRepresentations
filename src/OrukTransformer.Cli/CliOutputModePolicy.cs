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

        return TryParseLogLevel(logLevelRaw, out var parsed)
            ? parsed
            : LogLevel.Information;
    }

    internal static bool TryParseLogLevel(string value, out LogLevel logLevel)
    {
        switch (value.ToLowerInvariant())
        {
            case "trace":
                logLevel = LogLevel.Trace;
                return true;
            case "debug":
                logLevel = LogLevel.Debug;
                return true;
            case "information":
                logLevel = LogLevel.Information;
                return true;
            case "warning":
                logLevel = LogLevel.Warning;
                return true;
            case "error":
                logLevel = LogLevel.Error;
                return true;
            case "critical":
                logLevel = LogLevel.Critical;
                return true;
            case "none":
                logLevel = LogLevel.None;
                return true;
            default:
                logLevel = LogLevel.Information;
                return false;
        }
    }
}
