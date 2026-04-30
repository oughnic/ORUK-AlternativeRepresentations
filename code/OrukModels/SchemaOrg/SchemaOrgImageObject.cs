using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// An image file associated with a creative work, such as a photograph of
/// a building or a logo of an organisation.
/// Maps from ORUK <c>Organization.logo</c> (URL string).
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/ImageObject"/>
/// </remarks>
public record SchemaOrgImageObject
{
    /// <summary>Schema.org type discriminator — always <c>"ImageObject"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "ImageObject";

    /// <summary>
    /// The URL of the image file.
    /// Maps from ORUK <c>Organization.logo</c>.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>The width of the image in pixels.</summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; init; }

    /// <summary>The height of the image in pixels.</summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Height { get; init; }

    /// <summary>A caption for the image.</summary>
    [JsonPropertyName("caption")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Caption { get; init; }
}
