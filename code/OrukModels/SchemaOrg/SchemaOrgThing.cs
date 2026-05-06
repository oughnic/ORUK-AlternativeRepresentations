using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// Abstract base record for Schema.org node types that appear as top-level entries
/// in a JSON-LD <c>@graph</c> array (Thing and its subtypes).
///
/// Shared JSON-LD structural properties (<c>@id</c>, <c>name</c>, <c>description</c>,
/// <c>url</c>, <c>sameAs</c>, <c>image</c>, <c>identifier</c>,
/// <c>additionalType</c>, <c>additionalProperty</c>) are declared here so that
/// derived types inherit them without duplication.
///
/// The <c>@type</c> discriminator is managed by <see cref="JsonPolymorphicAttribute"/>
/// on this class.  Derived types do not declare a separate <c>@type</c> property.
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Thing"/>
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "@type")]
[JsonDerivedType(typeof(SchemaOrgService), typeDiscriminator: "Service")]
[JsonDerivedType(typeof(SchemaOrgGovernmentService), typeDiscriminator: "GovernmentService")]
[JsonDerivedType(typeof(SchemaOrgOrganization), typeDiscriminator: "Organization")]
[JsonDerivedType(typeof(SchemaOrgLocalBusiness), typeDiscriminator: "LocalBusiness")]
[JsonDerivedType(typeof(SchemaOrgPlace), typeDiscriminator: "Place")]
[JsonDerivedType(typeof(SchemaOrgAdministrativeArea), typeDiscriminator: "AdministrativeArea")]
public abstract record SchemaOrgThing
{
    /// <summary>
    /// The globally unique IRI (URI) for this node within the JSON-LD graph.
    /// Constructed as <c>&lt;baseUrl&gt;/services/&lt;id&gt;</c>, etc.
    /// </summary>
    [JsonPropertyName("@id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }

    /// <summary>
    /// The name of the item.
    /// <para>
    /// <b>Required</b> by Schema.org and Google Structured Data guidelines for
    /// <see cref="SchemaOrgService"/>, <see cref="SchemaOrgGovernmentService"/>,
    /// <see cref="SchemaOrgOrganization"/>, and <see cref="SchemaOrgLocalBusiness"/>.
    /// Always set this property for those types; omitting it will cause rich-result
    /// eligibility to fail.
    /// </para>
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>A short description of the item.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>URL of the item.</summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>
    /// URL of a reference web page that unambiguously indicates the item's identity
    /// (e.g. a Wikipedia page, official URI in a controlled vocabulary).
    /// </summary>
    [JsonPropertyName("sameAs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SameAs { get; init; }

    /// <summary>An image of the item.</summary>
    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgImageObject? Image { get; init; }

    /// <summary>
    /// An additional type for the item, used to indicate a more specific type
    /// from an external vocabulary.  Values should be URIs or
    /// <see cref="SchemaOrgDefinedTerm"/> nodes for labelled terms.
    /// </summary>
    [JsonPropertyName("additionalType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object>? AdditionalType { get; init; }

    /// <summary>
    /// A property-value pair representing an additional characteristic of the item.
    /// Use this for ORUK fields that have no direct Schema.org equivalent
    /// (e.g. <c>status</c>, <c>assuredDate</c>, <c>uprn</c>).
    /// </summary>
    [JsonPropertyName("additionalProperty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgPropertyValue>? AdditionalProperty { get; init; }

    /// <summary>
    /// The identifier property represents any kind of identifier for any kind of Thing.
    /// Use a <see cref="SchemaOrgPropertyValue"/> with <c>PropertyId</c> set
    /// (e.g. <c>"UPRN"</c>) for domain-specific identifiers.
    /// </summary>
    [JsonPropertyName("identifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgPropertyValue? Identifier { get; init; }

    /// <summary>
    /// The date and time when the item was most recently modified.
    /// Maps from ORUK <c>service.last_modified</c> (ISO 8601 timestamp).
    /// </summary>
    [JsonPropertyName("dateModified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateModified { get; init; }
}
