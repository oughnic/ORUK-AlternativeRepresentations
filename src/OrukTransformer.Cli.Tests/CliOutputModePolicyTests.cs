using Microsoft.Extensions.Logging;
using OrukTransformer.Cli;

namespace OrukTransformer.Cli.Tests;

public class CliOutputModePolicyTests
{
    [Fact]
    public void ValidateForOutputMode_NoJsonLdAndVerboseEnabled_ReturnsError()
    {
        var error = CliOutputModePolicy.ValidateForOutputMode(
            writingJsonToStdout: true,
            verboseEnabled: true,
            logLevelProvided: false,
            quietProvided: false);

        Assert.NotNull(error);
        Assert.Contains("--verbose", error);
    }

    [Fact]
    public void ValidateForOutputMode_NoJsonLdAndLogLevelProvided_ReturnsError()
    {
        var error = CliOutputModePolicy.ValidateForOutputMode(
            writingJsonToStdout: true,
            verboseEnabled: false,
            logLevelProvided: true,
            quietProvided: false);

        Assert.NotNull(error);
        Assert.Contains("--log-level", error);
    }

    [Fact]
    public void ResolveEffectiveLogLevel_NoJsonLd_AlwaysReturnsNone()
    {
        var level = CliOutputModePolicy.ResolveEffectiveLogLevel(
            writingJsonToStdout: true,
            quiet: false,
            logLevelRaw: "trace");

        Assert.Equal(LogLevel.None, level);
    }

    [Fact]
    public void ResolveEffectiveLogLevel_WithJsonLdAndQuiet_ReturnsWarning()
    {
        var level = CliOutputModePolicy.ResolveEffectiveLogLevel(
            writingJsonToStdout: false,
            quiet: true,
            logLevelRaw: "trace");

        Assert.Equal(LogLevel.Warning, level);
    }
}
