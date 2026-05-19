using System.Net;
using System.Text.RegularExpressions;

namespace OrukTransformer.Core;

/// <summary>
/// Normalizes ORUK free-text fields to plain text.
/// </summary>
public static partial class OrukPlainText
{
    [GeneratedRegex(@"\\+u([0-9A-Fa-f]{4})")]
    private static partial Regex UnicodeEscapePattern();

    [GeneratedRegex(@"<!--[\s\S]*?-->", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlCommentPattern();

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

        var text = DecodeUnicodeEscapes(value, maxPasses: 2);
        text = DecodeEntities(text, maxPasses: 2);

        if (HtmlCommentPattern().IsMatch(text))
            text = HtmlCommentPattern().Replace(text, " ");

        if (HtmlTagPattern().IsMatch(text))
            text = HtmlTagPattern().Replace(text, " ");

        text = DecodeEntities(text, maxPasses: 1);
        text = text.Replace('\u00A0', ' ');
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

    private static string DecodeEntities(string input, int maxPasses)
    {
        var text = input;
        for (var i = 0; i < maxPasses && text.Contains('&'); i++)
        {
            var decoded = WebUtility.HtmlDecode(text);
            if (string.Equals(decoded, text, StringComparison.Ordinal))
                break;
            text = decoded;
        }

        return text;
    }

    private static string DecodeUnicodeEscapes(string input, int maxPasses)
    {
        var text = input;
        for (var i = 0; i < maxPasses && text.Contains(@"\u", StringComparison.Ordinal); i++)
        {
            var decoded = UnicodeEscapePattern().Replace(text, static m =>
            {
                var codePoint = Convert.ToInt32(m.Groups[1].Value, 16);
                return ((char)codePoint).ToString();
            });

            if (string.Equals(decoded, text, StringComparison.Ordinal))
                break;

            text = decoded;
        }

        return text;
    }
}
