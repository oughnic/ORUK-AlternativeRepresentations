using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A JSON-LD document containing a Schema.org <c>@graph</c> of service-directory nodes.
///
/// Serialise this record to produce the canonical JSON-LD output:
/// <code>
/// {
///   "@context": "https://schema.org",
///   "@graph": [
///     { "@type": "GovernmentService", ... },
///     { "@type": "Organization", ... },
///     { "@type": "Place", ... }
///   ]
/// }
/// </code>
///
/// Use <see cref="SchemaOrgSerializerOptions.Default"/> when serialising to ensure
/// null properties are omitted and the <c>@type</c> discriminator is written correctly.
/// </summary>
public record SchemaOrgDocument
{
    /// <summary>
    /// The Schema.org context URI.  Always <c>"https://schema.org"</c>.
    /// </summary>
    [JsonPropertyName("@context")]
    public string Context { get; init; } = "https://schema.org";

    /// <summary>
    /// The collection of Schema.org nodes in this document.
    /// Typically contains one or more <see cref="SchemaOrgGovernmentService"/>,
    /// <see cref="SchemaOrgOrganization"/>, and <see cref="SchemaOrgPlace"/> nodes.
    /// </summary>
    [JsonPropertyName("@graph")]
    public IReadOnlyList<SchemaOrgThing> Graph { get; init; } = [];
}
