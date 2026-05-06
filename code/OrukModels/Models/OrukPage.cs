using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents a paginated ORUK v3 API response.
/// </summary>
/// <remarks>
/// Supports two paging strategies:
/// <list type="bullet">
/// <item><description>
///   <strong>RDPE (Realtime Paged Data Exchange)</strong> – the response contains a
///   <c>next_url</c> or <c>nextUrl</c> field that should be followed directly to
///   retrieve the next page.  Pagination ends when <see cref="NextUrl"/> is
///   <see langword="null"/> or empty.
/// </description></item>
/// <item><description>
///   <strong>Standard ORUK page-number paging</strong> – the response contains
///   <c>total_pages</c> and <c>page_number</c> fields; the caller increments the
///   <c>page</c> query parameter until all pages are consumed.
/// </description></item>
/// </list>
/// </remarks>
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

    /// <summary>
    /// RDPE next-page URL using the ORUK snake_case convention (<c>next_url</c>).
    /// </summary>
    [JsonPropertyName("next_url")]
    public string? NextUrlSnakeCase { get; init; }

    /// <summary>
    /// RDPE next-page URL using the camelCase convention (<c>nextUrl</c>), as
    /// returned by some RDPE-based feeds such as OpenSessions.
    /// </summary>
    [JsonPropertyName("nextUrl")]
    public string? NextUrlCamelCase { get; init; }

    /// <summary>
    /// The URL of the next page of results, or <see langword="null"/> when this
    /// is the last page.  Returns <see cref="NextUrlSnakeCase"/> if present,
    /// otherwise falls back to <see cref="NextUrlCamelCase"/>.
    /// </summary>
    [JsonIgnore]
    public string? NextUrl => NextUrlSnakeCase ?? NextUrlCamelCase;
}
