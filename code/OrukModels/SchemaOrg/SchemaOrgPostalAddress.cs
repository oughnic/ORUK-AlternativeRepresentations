using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A postal address.
/// Maps from an ORUK <c>Address</c> entity (the <c>physical_addresses</c> collection
/// on a <c>Location</c>).
///
/// <para>
/// <b>Required properties (Google Structured Data for LocalBusiness):</b>
/// <list type="bullet">
///   <item><see cref="StreetAddress"/></item>
///   <item><see cref="AddressLocality"/></item>
///   <item><see cref="PostalCode"/></item>
///   <item><see cref="AddressCountry"/> — ISO 3166-1 alpha-2 code (e.g. <c>"GB"</c>)</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/PostalAddress"/>
/// </remarks>
public record SchemaOrgPostalAddress
{
    /// <summary>Schema.org type discriminator — always <c>"PostalAddress"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "PostalAddress";

    /// <summary>
    /// The street address (house number and street name).
    /// Maps from ORUK <c>address_1</c>.
    /// <b>Required</b> by Google for local business rich results.
    /// </summary>
    [JsonPropertyName("streetAddress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StreetAddress { get; init; }

    /// <summary>
    /// The locality (city, town, or village).
    /// Maps from ORUK <c>city</c>.
    /// <b>Required</b> by Google for local business rich results.
    /// </summary>
    [JsonPropertyName("addressLocality")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLocality { get; init; }

    /// <summary>
    /// The region, county, or state.
    /// Maps from ORUK <c>state_province</c>.
    /// </summary>
    [JsonPropertyName("addressRegion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressRegion { get; init; }

    /// <summary>
    /// The postal code (postcode).
    /// Maps from ORUK <c>postal_code</c>.
    /// <b>Required</b> by Google for local business rich results.
    /// </summary>
    [JsonPropertyName("postalCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PostalCode { get; init; }

    /// <summary>
    /// The ISO 3166-1 alpha-2 country code (e.g. <c>"GB"</c> for the United Kingdom).
    /// Maps from ORUK <c>country</c>.
    /// <b>Required</b> by Google for local business rich results.
    /// </summary>
    [JsonPropertyName("addressCountry")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressCountry { get; init; }
}
