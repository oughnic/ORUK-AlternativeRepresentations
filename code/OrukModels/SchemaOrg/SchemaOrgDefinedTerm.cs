using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A word, name, acronym, phrase, etc. with a formal definition.
/// Used when a taxonomy term has a known URI but is not a standard Schema.org type.
/// Maps from an ORUK <c>TaxonomyTerm</c> when <c>term_uri</c> is present.
/// </summary>
/// <remarks>
/// Example JSON-LD:
/// <code>
/// {
///   "@type": "DefinedTerm",
///   "@id": "https://standards.esd.org.uk/?uri=esd%3Aservice%2F841",
///   "name": "Day Opportunities",
///   "inDefinedTermSet": "https://standards.esd.org.uk/"
/// }
/// </code>
/// Reference: <see href="https://schema.org/DefinedTerm"/>
/// </remarks>
public record SchemaOrgDefinedTerm
{
    /// <summary>Schema.org type discriminator — always <c>"DefinedTerm"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "DefinedTerm";

    /// <summary>
    /// The URI that uniquely identifies this term within its vocabulary.
    /// Maps from ORUK <c>TaxonomyTerm.term_uri</c>.
    /// </summary>
    [JsonPropertyName("@id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }

    /// <summary>
    /// The human-readable label for this term.
    /// Maps from ORUK <c>TaxonomyTerm.name</c>.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// A <see cref="string"/> URI for the defined term set (vocabulary) to which this term belongs.
    /// Examples: <c>"https://standards.esd.org.uk/"</c>, <c>"http://snomed.info/sct"</c>.
    /// </summary>
    [JsonPropertyName("inDefinedTermSet")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InDefinedTermSet { get; init; }

    /// <summary>
    /// The code within the vocabulary (e.g. SNOMED CT concept ID, ESD code).
    /// Maps from ORUK <c>TaxonomyTerm.code</c>.
    /// </summary>
    [JsonPropertyName("termCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TermCode { get; init; }
}
