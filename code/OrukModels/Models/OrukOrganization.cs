using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Organization — a legal entity that delivers services.
/// Navigation properties are virtual for future Entity Framework Core integration.
/// </summary>
public class OrukOrganization
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("alternate_name")]
    public string? AlternateName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Bristol OPD uses "website" rather than "url" in the organization object.
    /// Both are captured here to support liberal receive semantics.
    /// </summary>
    [JsonPropertyName("website")]
    public string? Website { get; set; }

    /// <summary>Linked-data URI for the organisation (ORUK extension).</summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("legal_status")]
    public string? LegalStatus { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }

    [JsonPropertyName("year_incorporated")]
    public string? YearIncorporated { get; set; }

    [JsonPropertyName("parent_organization_id")]
    public string? ParentOrganizationId { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────────

    [JsonPropertyName("services")]
    public virtual ICollection<OrukService> Services { get; set; } = [];

    [JsonPropertyName("contacts")]
    public virtual ICollection<OrukContact> Contacts { get; set; } = [];

    [JsonPropertyName("phones")]
    public virtual ICollection<OrukPhone> Phones { get; set; } = [];

    [JsonPropertyName("locations")]
    public virtual ICollection<OrukLocation> Locations { get; set; } = [];

    [JsonPropertyName("funding")]
    public virtual ICollection<OrukFunding> Funding { get; set; } = [];

    [JsonPropertyName("programs")]
    public virtual ICollection<OrukProgram> Programs { get; set; } = [];

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
