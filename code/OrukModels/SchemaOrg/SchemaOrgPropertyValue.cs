using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A property-value pair representing an additional characteristic of an item.
/// Used to preserve ORUK fields that have no direct Schema.org equivalent
/// (e.g. <c>status</c>, <c>assuredDate</c>, <c>uprn</c>).
///
/// <para>
/// <b>Both <see cref="Name"/> and <see cref="Value"/> are required.</b>
/// A <see cref="SchemaOrgPropertyValue"/> with either field missing is not valid Schema.org.
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/PropertyValue"/>
/// </remarks>
public record SchemaOrgPropertyValue
{
    /// <summary>Schema.org type discriminator — <c>"PropertyValue"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "PropertyValue";

    /// <summary>
    /// The name of the property (e.g. <c>"uprn"</c>, <c>"orukStatus"</c>).
    /// <b>Required.</b>
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The value of the property.
    /// <b>Required.</b>
    /// May be a string, number, boolean, or structured object.
    /// </summary>
    [JsonPropertyName("value")]
    public required object Value { get; init; }

    /// <summary>
    /// A commonly used identifier for the characteristic represented by the property,
    /// e.g. a property ID from an external vocabulary.
    /// Examples: <c>"UPRN"</c>, <c>"USRN"</c>.
    /// </summary>
    [JsonPropertyName("propertyID")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PropertyId { get; init; }
}

/// <summary>
/// A feature of a location, used to describe accessibility provisions
/// (e.g. wheelchair access, hearing loop, accessible parking).
/// Maps from an ORUK <c>Accessibility</c> entity.
///
/// <para>
/// <b>Both <see cref="SchemaOrgPropertyValue.Name"/> and
/// <see cref="SchemaOrgPropertyValue.Value"/> are required.</b>
/// </para>
/// </summary>
/// <remarks>
/// <see cref="SchemaOrgLocationFeatureSpecification"/> extends
/// <see cref="SchemaOrgPropertyValue"/> and is used as the element type for
/// <see cref="SchemaOrgPlace.AmenityFeature"/>.
/// Reference: <see href="https://schema.org/LocationFeatureSpecification"/>
/// </remarks>
public record SchemaOrgLocationFeatureSpecification : SchemaOrgPropertyValue
{
    /// <summary>Schema.org type discriminator — always <c>"LocationFeatureSpecification"</c>.</summary>
    [JsonPropertyName("@type")]
    public new string Type { get; } = "LocationFeatureSpecification";

    /// <summary>
    /// The date from which this feature is available (ISO 8601 date).
    /// </summary>
    [JsonPropertyName("validFrom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValidFrom { get; init; }

    /// <summary>
    /// The date until which this feature is available (ISO 8601 date).
    /// </summary>
    [JsonPropertyName("validThrough")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValidThrough { get; init; }
}
