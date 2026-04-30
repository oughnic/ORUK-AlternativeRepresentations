using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Service entity — the core unit of the service directory.
/// Property names are mapped to the ORUK JSON snake_case format via <see cref="JsonPropertyNameAttribute"/>.
/// Navigation properties are virtual to support future Entity Framework Core integration.
/// </summary>
public class OrukService
{
    // ── Scalar fields ────────────────────────────────────────────────────────────

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonPropertyName("program_id")]
    public string? ProgramId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("alternate_name")]
    public string? AlternateName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// One of: active, inactive, defunct, temporarily closed.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("interpretation_services")]
    public string? InterpretationServices { get; set; }

    [JsonPropertyName("application_process")]
    public string? ApplicationProcess { get; set; }

    [JsonPropertyName("fees_description")]
    public string? FeesDescription { get; set; }

    /// <summary>DEPRECATED in HSDS v3. Use <see cref="FeesDescription"/>.</summary>
    [JsonPropertyName("fees")]
    public string? Fees { get; set; }

    /// <summary>DEPRECATED in HSDS v3. Use schedule information instead.</summary>
    [JsonPropertyName("wait_time")]
    public string? WaitTime { get; set; }

    [JsonPropertyName("accreditations")]
    public string? Accreditations { get; set; }

    [JsonPropertyName("eligibility_description")]
    public string? EligibilityDescription { get; set; }

    [JsonPropertyName("minimum_age")]
    public double? MinimumAge { get; set; }

    [JsonPropertyName("maximum_age")]
    public double? MaximumAge { get; set; }

    [JsonPropertyName("assured_date")]
    public string? AssuredDate { get; set; }

    [JsonPropertyName("assurer_email")]
    public string? AssurerEmail { get; set; }

    /// <summary>DEPRECATED in HSDS v3.</summary>
    [JsonPropertyName("licenses")]
    public string? Licenses { get; set; }

    [JsonPropertyName("alert")]
    public string? Alert { get; set; }

    [JsonPropertyName("last_modified")]
    public string? LastModified { get; set; }

    // ── Navigation properties (virtual for EF Core compatibility) ────────────────

    [JsonPropertyName("organization")]
    public virtual OrukOrganization? Organization { get; set; }

    [JsonPropertyName("program")]
    public virtual OrukProgram? Program { get; set; }

    [JsonPropertyName("contacts")]
    public virtual ICollection<OrukContact> Contacts { get; set; } = [];

    [JsonPropertyName("phones")]
    public virtual ICollection<OrukPhone> Phones { get; set; } = [];

    [JsonPropertyName("schedules")]
    public virtual ICollection<OrukSchedule> Schedules { get; set; } = [];

    [JsonPropertyName("service_areas")]
    public virtual ICollection<OrukServiceArea> ServiceAreas { get; set; } = [];

    [JsonPropertyName("service_at_locations")]
    public virtual ICollection<OrukServiceAtLocation> ServiceAtLocations { get; set; } = [];

    [JsonPropertyName("languages")]
    public virtual ICollection<OrukLanguage> Languages { get; set; } = [];

    [JsonPropertyName("cost_options")]
    public virtual ICollection<OrukCostOption> CostOptions { get; set; } = [];

    [JsonPropertyName("eligibility")]
    public virtual ICollection<OrukEligibility> Eligibility { get; set; } = [];

    [JsonPropertyName("required_documents")]
    public virtual ICollection<OrukRequiredDocument> RequiredDocuments { get; set; } = [];

    [JsonPropertyName("funding")]
    public virtual ICollection<OrukFunding> Funding { get; set; } = [];

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];

    // ── Vendor extension data ─────────────────────────────────────────────────────
    // Any JSON fields not mapped to known ORUK / HSDS properties are captured here.
    // Use the Extensions property to enumerate them as structured OrukExtensionProperty
    // instances with namespace, key, and raw JSON value.

    /// <summary>
    /// Raw dictionary of any unrecognised JSON fields from the feed.
    /// Populated automatically by System.Text.Json for all properties not explicitly
    /// declared on this class.  Use <see cref="Extensions"/> for structured access.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// Structured view of all vendor-extended fields captured in <see cref="ExtensionData"/>.
    /// Each entry exposes the <c>Namespace</c>, <c>Key</c>, <c>RawKey</c>, and raw
    /// <see cref="JsonElement"/> <c>Value</c>.
    /// Returns an empty list when no extension data is present.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<OrukExtensionProperty> Extensions =>
        ExtensionData is null
            ? []
            : [.. ExtensionData.Select(kvp => OrukExtensionProperty.FromRawKey(kvp.Key, kvp.Value))];
}
