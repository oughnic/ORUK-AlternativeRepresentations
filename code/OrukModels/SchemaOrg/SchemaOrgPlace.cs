using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// Entities that have a somewhat fixed, physical extension (a building, park, room, etc.).
/// Maps from an ORUK <c>Location</c> entity.
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Place"/>
/// </remarks>
public record SchemaOrgPlace : SchemaOrgThing
{
    /// <summary>
    /// The postal address of the place.
    /// Maps from ORUK <c>physical_addresses</c>.
    /// </summary>
    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgPostalAddress? Address { get; init; }

    /// <summary>
    /// The geographic coordinates of the place (latitude and longitude).
    /// Maps from ORUK <c>latitude</c> and <c>longitude</c>.
    /// </summary>
    [JsonPropertyName("geo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgGeoCoordinates? Geo { get; init; }

    /// <summary>
    /// An amenity feature (e.g. a characteristic or service) of the accommodation.
    /// Use <see cref="SchemaOrgLocationFeatureSpecification"/> to describe
    /// accessibility features. Maps from ORUK <c>accessibility</c>.
    /// </summary>
    [JsonPropertyName("amenityFeature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgLocationFeatureSpecification>? AmenityFeature { get; init; }

    /// <summary>
    /// The telephone number of the place.
    /// Include the country dialling code where possible.
    /// </summary>
    [JsonPropertyName("telephone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Telephone { get; init; }

    /// <summary>An alternate name for the place.</summary>
    [JsonPropertyName("alternateName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlternateName { get; init; }

    /// <summary>Opening hours of the place.</summary>
    [JsonPropertyName("openingHoursSpecification")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgOpeningHoursSpecification>? OpeningHoursSpecification { get; init; }
}

/// <summary>
/// A geographical region, typically under the jurisdiction of a particular government.
/// Used to represent service coverage areas (e.g. a local authority district).
/// Maps from ORUK <c>ServiceArea</c>.
/// </summary>
/// <remarks>
/// <see cref="SchemaOrgAdministrativeArea"/> extends <see cref="SchemaOrgPlace"/>
/// and inherits all its properties.
/// Reference: <see href="https://schema.org/AdministrativeArea"/>
/// </remarks>
public record SchemaOrgAdministrativeArea : SchemaOrgPlace;
