using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Contact associated with a service, organisation, or location.
/// Navigation properties are virtual for future Entity Framework Core integration.
/// </summary>
public class OrukContact
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonPropertyName("service_at_location_id")]
    public string? ServiceAtLocationId { get; set; }

    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("department")]
    public string? Department { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────────

    [JsonPropertyName("phones")]
    public virtual ICollection<OrukPhone> Phones { get; set; } = [];

    [JsonPropertyName("languages")]
    public virtual ICollection<OrukLanguage> Languages { get; set; } = [];

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
