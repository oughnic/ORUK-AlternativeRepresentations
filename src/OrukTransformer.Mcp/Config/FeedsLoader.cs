using System.Text.Json;

namespace OrukTransformer.Mcp.Config;

/// <summary>
/// Reads the project-level <c>feeds.json</c> file and extracts ORUK endpoint base URLs.
/// Supports both simple string entries and object entries with a <c>url</c> field,
/// matching the format described in the project configuration documentation.
/// </summary>
internal static class FeedsLoader
{
    /// <summary>
    /// Attempts to load feed URLs from the specified <paramref name="feedsJsonPath"/>.
    /// Returns an empty list if the file does not exist or cannot be parsed.
    /// </summary>
    internal static IReadOnlyList<Uri> LoadFeedUrls(string feedsJsonPath)
    {
        if (!File.Exists(feedsJsonPath))
            return [];

        try
        {
            var json = File.ReadAllText(feedsJsonPath);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return [];

            var urls = new List<Uri>();
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var raw = element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Object when element.TryGetProperty("url", out var urlProp)
                        => urlProp.GetString(),
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(raw) &&
                    Uri.TryCreate(raw, UriKind.Absolute, out var uri))
                {
                    urls.Add(uri);
                }
            }

            return urls.AsReadOnly();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return [];
        }
    }
}
