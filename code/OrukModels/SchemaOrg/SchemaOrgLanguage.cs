using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A natural language or programming language.
/// Used to represent languages in which a service or contact point is available.
/// Maps from an ORUK <c>Language</c> entity or <c>interpretation_services</c>.
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Language"/>
/// </remarks>
public record SchemaOrgLanguage
{
    /// <summary>Schema.org type discriminator — always <c>"Language"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "Language";

    /// <summary>
    /// The name of the language (e.g. <c>"Welsh"</c>, <c>"British Sign Language"</c>).
    /// Maps from ORUK <c>Language.name</c>.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// A commonly used identifier for the language, typically an ISO 639-1 or BCP 47 code
    /// (e.g. <c>"cy"</c> for Welsh, <c>"bfi"</c> for British Sign Language).
    /// Maps from ORUK <c>Language.code</c>.
    /// </summary>
    [JsonPropertyName("identifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Identifier { get; init; }
}
