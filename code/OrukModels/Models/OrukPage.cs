using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents a paginated ORUK v3 API response.
/// </summary>
/// <typeparam name="T">The type of items in the <see cref="Contents"/> collection.</typeparam>
public record OrukPage<T>
{
    [JsonPropertyName("total_items")]
    public int TotalItems { get; init; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("page_number")]
    public int PageNumber { get; init; }

    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("first_page")]
    public bool FirstPage { get; init; }

    [JsonPropertyName("last_page")]
    public bool LastPage { get; init; }

    [JsonPropertyName("empty")]
    public bool Empty { get; init; }

    [JsonPropertyName("contents")]
    public IReadOnlyList<T> Contents { get; init; } = [];
}
