using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS RequiredDocument — a document a service user
/// must present to access a service.
/// </summary>
public class OrukRequiredDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("document")]
    public string? Document { get; set; }

    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
