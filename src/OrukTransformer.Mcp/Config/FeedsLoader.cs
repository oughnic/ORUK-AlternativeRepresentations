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
    internal static IReadOnlyList<FeedDefinition> LoadFeeds(string feedsJsonPath)
    {
        if (!File.Exists(feedsJsonPath))
            return [];

        try
        {
            var json = File.ReadAllText(feedsJsonPath);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                Console.Error.WriteLine(
                    $"[FeedsLoader] ERROR: '{feedsJsonPath}' must contain a JSON array; found {doc.RootElement.ValueKind}.");
                return [];
            }

            var feeds = new List<FeedDefinition>();
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var feed = element.ValueKind switch
                {
                    JsonValueKind.String when Uri.TryCreate(element.GetString(), UriKind.Absolute, out var uri)
                        => new FeedDefinition(uri),
                    JsonValueKind.Object when element.TryGetProperty("url", out var urlProp)
                        && Uri.TryCreate(urlProp.GetString(), UriKind.Absolute, out var uri)
                        => new FeedDefinition(
                            uri,
                            element.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
                            element.TryGetProperty("aliases", out var aliasesProp) && aliasesProp.ValueKind == JsonValueKind.Array
                                ? aliasesProp.EnumerateArray()
                                    .Select(alias => alias.GetString())
                                    .Where(alias => !string.IsNullOrWhiteSpace(alias))
                                    .Select(alias => alias!)
                                    .ToList()
                                : null),
                    _ => null
                };

                if (feed is not null)
                    feeds.Add(feed);
            }

            return feeds.AsReadOnly();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            Console.Error.WriteLine(
                $"[FeedsLoader] ERROR: Could not read or parse '{feedsJsonPath}': {ex.Message}");
            return [];
        }
    }

    internal static IReadOnlyList<Uri> LoadFeedUrls(string feedsJsonPath)
        => LoadFeeds(feedsJsonPath).Select(feed => feed.Url).ToList().AsReadOnly();
}
