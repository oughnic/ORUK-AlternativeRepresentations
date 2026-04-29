using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Attribute — a link between an entity and a taxonomy term
/// that classifies or describes the entity (e.g. service category, eligibility type).
/// </summary>
public class OrukAttribute
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The identifier of the entity to which this taxonomy term applies.</summary>
    [JsonPropertyName("link_id")]
    public string? LinkId { get; set; }

    [JsonPropertyName("taxonomy_term_id")]
    public string? TaxonomyTermId { get; set; }

    /// <summary>
    /// An open-codelist code describing what the taxonomy term represents
    /// (e.g. "service", "eligibility", "intended audience").
    /// </summary>
    [JsonPropertyName("link_type")]
    public string? LinkType { get; set; }

    /// <summary>The name of the entity table being linked (e.g. "service", "location").</summary>
    [JsonPropertyName("link_entity")]
    public string? LinkEntity { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────────

    [JsonPropertyName("taxonomy_term")]
    public virtual OrukTaxonomyTerm? TaxonomyTerm { get; set; }

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
