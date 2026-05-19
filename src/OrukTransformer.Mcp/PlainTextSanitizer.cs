using OrukTransformer.Core;

namespace OrukTransformer.Mcp;

/// <summary>
/// Backward-compatible wrapper over the shared plain-text normalizer in OrukTransformer.Core.
/// </summary>
public static class PlainTextSanitizer
{
    public static string? ToPlainText(string? value) => OrukPlainText.ToPlainText(value);

    public static string? ToPlainTextAndTruncate(string? value, int maxLength) =>
        OrukPlainText.ToPlainTextAndTruncate(value, maxLength);
}
