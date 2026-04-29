using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS TaxonomyTerm — a category or classification applied
/// to services, locations, and other entities.
/// </summary>
public class OrukTaxonomyTerm
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The term identifier as used within the taxonomy.
    /// Combined with <see cref="TaxonomyId"/> this uniquely identifies the term.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Identifier of the parent term (for hierarchical taxonomies).</summary>
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Free-text name of the taxonomy in use. Provide a URI if possible.
    /// Known values: "esdstandards", "snomed", "loinc", "icd-10".
    /// </summary>
    [JsonPropertyName("taxonomy")]
    public string? Taxonomy { get; set; }

    [JsonPropertyName("taxonomy_id")]
    public string? TaxonomyId { get; set; }

    /// <summary>Resolvable URI for the term (e.g. an ESD Standards or SNOMED URI).</summary>
    [JsonPropertyName("term_uri")]
    public string? TermUri { get; set; }

    /// <summary>ISO 639-1/2 language code for the term label.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    // ── Navigation properties ────────────────────────────────────────────────────

    [JsonPropertyName("taxonomy_detail")]
    public virtual OrukTaxonomy? TaxonomyDetail { get; set; }

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
