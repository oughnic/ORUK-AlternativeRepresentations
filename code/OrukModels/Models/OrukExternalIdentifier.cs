using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an external identifier associated with a location (e.g. UPRN, USRN).
/// Used to store the UK Unique Property Reference Number and similar external codes
/// as structured data rather than free-text fields.
/// </summary>
public class OrukExternalIdentifier
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    /// <summary>
    /// The identifier value (e.g. the UPRN number string).
    /// </summary>
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }

    /// <summary>
    /// The identifier scheme (e.g. "UPRN", "USRN").
    /// </summary>
    [JsonPropertyName("identifier_scheme")]
    public string? IdentifierScheme { get; set; }

    [JsonPropertyName("identifier_type")]
    public string? IdentifierType { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
