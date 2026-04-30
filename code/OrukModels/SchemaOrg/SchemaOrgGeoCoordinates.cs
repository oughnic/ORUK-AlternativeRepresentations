using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// The geographic coordinates of a place or event.
/// Maps from ORUK <c>Location.latitude</c> and <c>Location.longitude</c>.
///
/// <para>
/// <b>Both <see cref="Latitude"/> and <see cref="Longitude"/> are required.</b>
/// A <see cref="SchemaOrgGeoCoordinates"/> without a location makes no semantic sense.
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/GeoCoordinates"/>
/// </remarks>
public record SchemaOrgGeoCoordinates
{
    /// <summary>Schema.org type discriminator — always <c>"GeoCoordinates"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "GeoCoordinates";

    /// <summary>
    /// The latitude of a location in decimal degrees (WGS 84).
    /// <b>Required.</b>
    /// </summary>
    [JsonPropertyName("latitude")]
    public required decimal Latitude { get; init; }

    /// <summary>
    /// The longitude of a location in decimal degrees (WGS 84).
    /// <b>Required.</b>
    /// </summary>
    [JsonPropertyName("longitude")]
    public required decimal Longitude { get; init; }
}
