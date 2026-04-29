using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS ServiceAtLocation — the join between a service
/// and a specific location at which it is delivered.
/// Navigation properties are virtual for future Entity Framework Core integration.
/// </summary>
public class OrukServiceAtLocation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────────

    [JsonPropertyName("location")]
    public virtual OrukLocation? Location { get; set; }

    [JsonPropertyName("contacts")]
    public virtual ICollection<OrukContact> Contacts { get; set; } = [];

    [JsonPropertyName("phones")]
    public virtual ICollection<OrukPhone> Phones { get; set; } = [];

    [JsonPropertyName("schedules")]
    public virtual ICollection<OrukSchedule> Schedules { get; set; } = [];

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
