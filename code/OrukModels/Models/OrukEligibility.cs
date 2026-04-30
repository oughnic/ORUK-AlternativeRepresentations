using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Eligibility record defining who can access a service.
/// </summary>
public class OrukEligibility
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("minimum_age")]
    public double? MinimumAge { get; set; }

    [JsonPropertyName("maximum_age")]
    public double? MaximumAge { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
