using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Location — a physical address or geographic point
/// where services are delivered.
/// Navigation properties are virtual for future Entity Framework Core integration.
/// </summary>
public class OrukLocation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("location_type")]
    public string? LocationType { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("alternate_name")]
    public string? AlternateName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("transportation")]
    public string? Transportation { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    /// <summary>UK Unique Property Reference Number (ORUK required field).</summary>
    [JsonPropertyName("uprn")]
    public string? Uprn { get; set; }

    /// <summary>UK Unique Street Reference Number (ORUK optional field).</summary>
    [JsonPropertyName("usrn")]
    public string? Usrn { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────────

    [JsonPropertyName("physical_addresses")]
    public virtual ICollection<OrukAddress> PhysicalAddresses { get; set; } = [];

    [JsonPropertyName("postal_addresses")]
    public virtual ICollection<OrukAddress> PostalAddresses { get; set; } = [];

    [JsonPropertyName("contacts")]
    public virtual ICollection<OrukContact> Contacts { get; set; } = [];

    [JsonPropertyName("phones")]
    public virtual ICollection<OrukPhone> Phones { get; set; } = [];

    [JsonPropertyName("schedules")]
    public virtual ICollection<OrukSchedule> Schedules { get; set; } = [];

    [JsonPropertyName("languages")]
    public virtual ICollection<OrukLanguage> Languages { get; set; } = [];

    [JsonPropertyName("accessibility")]
    public virtual ICollection<OrukAccessibility> Accessibility { get; set; } = [];

    [JsonPropertyName("external_identifiers")]
    public virtual ICollection<OrukExternalIdentifier> ExternalIdentifiers { get; set; } = [];

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
