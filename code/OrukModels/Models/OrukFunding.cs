using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Funding record for a service or organisation.
/// </summary>
public class OrukFunding
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
