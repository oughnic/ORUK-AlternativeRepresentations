using System.Net;
using System.Text.RegularExpressions;

namespace OrukTransformer.Mcp;

/// <summary>
/// Converts ORUK free-text fields to plain text for MCP responses.
/// Removes HTML tags, decodes entities, and normalises whitespace.
/// </summary>
public static partial class PlainTextSanitizer
{
    [GeneratedRegex(@"<[^>]+>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultipleWhitespacePattern();

    [GeneratedRegex(@"\s+([,.;:!?])")]
    private static partial Regex SpaceBeforePunctuationPattern();

    public static string? ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value;

        if (HtmlTagPattern().IsMatch(text))
            text = HtmlTagPattern().Replace(text, " ");

        if (text.Contains('&'))
            text = WebUtility.HtmlDecode(text);

        text = MultipleWhitespacePattern().Replace(text, " ").Trim();
        text = SpaceBeforePunctuationPattern().Replace(text, "$1");
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public static string? ToPlainTextAndTruncate(string? value, int maxLength)
    {
        var plain = ToPlainText(value);
        if (string.IsNullOrWhiteSpace(plain))
            return null;

        return plain.Length <= maxLength
            ? plain
            : plain[..maxLength].TrimEnd() + "…";
    }
}
