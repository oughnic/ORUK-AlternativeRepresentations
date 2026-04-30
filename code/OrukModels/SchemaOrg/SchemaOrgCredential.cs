using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// An educational or occupational credential that certifies a qualification, skill, or achievement.
/// Used to represent accreditations held by a service or organisation.
/// Maps from ORUK <c>Service.accreditations</c>.
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/EducationalOccupationalCredential"/>
/// </remarks>
public record SchemaOrgEducationalOccupationalCredential
{
    /// <summary>
    /// Schema.org type discriminator — always <c>"EducationalOccupationalCredential"</c>.
    /// </summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "EducationalOccupationalCredential";

    /// <summary>
    /// The name of the credential or accreditation.
    /// Maps from ORUK <c>Service.accreditations</c> (free-text field).
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// The organisation that issued the credential.
    /// </summary>
    [JsonPropertyName("recognizedBy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgOrganization? RecognizedBy { get; init; }

    /// <summary>
    /// URL of the credential or the body that awards it.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }
}
