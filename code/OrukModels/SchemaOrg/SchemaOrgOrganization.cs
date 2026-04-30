using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// An organisation such as a school, NGO, corporation, club, or government agency.
///
/// <para>
/// <b>Required properties (Schema.org + Google Structured Data):</b>
/// <see cref="SchemaOrgThing.Name"/> — always set this for rich-result eligibility.
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Organization"/>
/// </remarks>
public record SchemaOrgOrganization : SchemaOrgThing
{
    /// <summary>An alternate name for the organisation.</summary>
    [JsonPropertyName("alternateName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlternateName { get; init; }

    /// <summary>Email address for the organisation.</summary>
    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; init; }

    /// <summary>
    /// The official name of the organisation (legal name, if applicable).
    /// Maps from ORUK <c>legal_status</c> when a formal legal name is present.
    /// </summary>
    [JsonPropertyName("legalName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegalName { get; init; }

    /// <summary>
    /// An associated logo for the organisation.
    /// Maps from ORUK <c>logo</c>.
    /// </summary>
    [JsonPropertyName("logo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgImageObject? Logo { get; init; }

    /// <summary>
    /// The date that this organisation was founded.
    /// Maps from ORUK <c>year_incorporated</c>.
    /// Use ISO 8601 format (e.g. <c>"2003"</c> or <c>"2003-06-01"</c>).
    /// </summary>
    [JsonPropertyName("foundingDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FoundingDate { get; init; }

    /// <summary>
    /// The larger organisation that this organisation is a sub-organisation of.
    /// Typically an <c>@id</c> reference to another <see cref="SchemaOrgOrganization"/> node.
    /// </summary>
    [JsonPropertyName("parentOrganization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ParentOrganization { get; init; }

    /// <summary>Contact points for the organisation.</summary>
    [JsonPropertyName("contactPoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgContactPoint>? ContactPoint { get; init; }

    /// <summary>Services provided by this organisation.</summary>
    [JsonPropertyName("hasOfferCatalog")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object>? HasOfferCatalog { get; init; }
}

/// <summary>
/// A local business — a physically located organisation that delivers services
/// at a specific address.
///
/// <para>
/// <b>Required properties (Schema.org + Google Structured Data):</b>
/// <list type="bullet">
///   <item><see cref="SchemaOrgThing.Name"/></item>
///   <item><see cref="Address"/> (strongly recommended for rich results)</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// <see cref="SchemaOrgLocalBusiness"/> extends <see cref="SchemaOrgOrganization"/>
/// and also represents a <c>Place</c> in Schema.org's multiple-inheritance model.
/// Place-specific properties (address, geo, opening hours) are included here.
/// Reference: <see href="https://schema.org/LocalBusiness"/>
/// </remarks>
public record SchemaOrgLocalBusiness : SchemaOrgOrganization
{
    /// <summary>
    /// The postal address of the business.
    /// <b>Required</b> by Google Structured Data guidelines for local business rich results.
    /// </summary>
    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgPostalAddress? Address { get; init; }

    /// <summary>Geographic coordinates of the business location.</summary>
    [JsonPropertyName("geo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgGeoCoordinates? Geo { get; init; }

    /// <summary>Opening hours specifications for the business.</summary>
    [JsonPropertyName("openingHoursSpecification")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgOpeningHoursSpecification>? OpeningHoursSpecification { get; init; }

    /// <summary>
    /// The telephone number of the business.
    /// Include the country dialling code where possible (e.g. <c>"+44 117 123 4567"</c>).
    /// </summary>
    [JsonPropertyName("telephone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Telephone { get; init; }

    /// <summary>The currencies accepted by the business (free-text, e.g. <c>"GBP"</c>).</summary>
    [JsonPropertyName("currenciesAccepted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrenciesAccepted { get; init; }

    /// <summary>Cash, credit card, or other payment methods accepted (free-text).</summary>
    [JsonPropertyName("paymentAccepted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PaymentAccepted { get; init; }

    /// <summary>The price range of the business (e.g. <c>"£"</c>, <c>"££"</c>, <c>"Free"</c>).</summary>
    [JsonPropertyName("priceRange")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PriceRange { get; init; }
}
