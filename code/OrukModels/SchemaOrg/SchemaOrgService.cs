using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A service provided by an organisation.
///
/// <para>
/// <b>Required properties (Schema.org + Google Structured Data):</b>
/// <see cref="SchemaOrgThing.Name"/> — always set this for rich-result eligibility.
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Service"/>
/// </remarks>
public record SchemaOrgService : SchemaOrgThing
{
    /// <summary>An alternate name for the service.</summary>
    [JsonPropertyName("alternateName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlternateName { get; init; }

    /// <summary>Email address for the service.</summary>
    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; init; }

    /// <summary>
    /// The organisation or person providing the service.
    /// Typically an <c>@id</c> reference to an <see cref="SchemaOrgOrganization"/> node
    /// in the same <c>@graph</c>.
    /// </summary>
    [JsonPropertyName("provider")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Provider { get; init; }

    /// <summary>
    /// The geographic area(s) where the service is available.
    /// Use <see cref="SchemaOrgAdministrativeArea"/> for named areas or
    /// a <see cref="SchemaOrgPlace"/> for a specific location.
    /// </summary>
    [JsonPropertyName("areaServed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object>? AreaServed { get; init; }

    /// <summary>
    /// The intended audience for the service (eligibility criteria).
    /// Use <see cref="SchemaOrgPeopleAudience"/> when age constraints apply.
    /// </summary>
    [JsonPropertyName("audience")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgAudience? Audience { get; init; }

    /// <summary>
    /// Channels through which the service is available
    /// (e.g. phone, online, in-person).
    /// </summary>
    [JsonPropertyName("availableChannel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgServiceChannel>? AvailableChannel { get; init; }

    /// <summary>
    /// Languages in which the service is available.
    /// Maps from ORUK <c>interpretation_services</c> or <c>languages</c>.
    /// </summary>
    [JsonPropertyName("availableLanguage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgLanguage>? AvailableLanguage { get; init; }

    /// <summary>
    /// Contact point(s) for the service (phone, email, web form).
    /// </summary>
    [JsonPropertyName("contactPoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgContactPoint>? ContactPoint { get; init; }

    /// <summary>
    /// Credentials or accreditations held by the service provider or the service itself.
    /// Maps from ORUK <c>accreditations</c>.
    /// </summary>
    [JsonPropertyName("hasCredential")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgEducationalOccupationalCredential>? HasCredential { get; init; }

    /// <summary>
    /// Keywords or phrases describing the service.
    /// Taxonomy term names and hierarchy paths are concatenated here.
    /// </summary>
    [JsonPropertyName("keywords")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Keywords { get; init; }

    /// <summary>
    /// The location(s) where the service is delivered.
    /// Typically an <c>@id</c> reference to a <see cref="SchemaOrgPlace"/> node.
    /// </summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object>? Location { get; init; }

    /// <summary>
    /// Pricing or access offers for the service.
    /// Maps from ORUK <c>cost_options</c>.
    /// </summary>
    [JsonPropertyName("offers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgOffer>? Offers { get; init; }

    /// <summary>
    /// Opening-hours specifications for when the service is available.
    /// Maps from ORUK <c>schedules</c>.
    /// </summary>
    [JsonPropertyName("openingHoursSpecification")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgOpeningHoursSpecification>? OpeningHoursSpecification { get; init; }

    /// <summary>
    /// The type of service (free-text label or controlled-vocabulary term).
    /// Maps from ORUK service category or taxonomy.
    /// </summary>
    [JsonPropertyName("serviceType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServiceType { get; init; }

    /// <summary>
    /// Human-readable terms of service or access conditions.
    /// Maps from ORUK <c>application_process</c>.
    /// </summary>
    [JsonPropertyName("termsOfService")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TermsOfService { get; init; }
}

/// <summary>
/// A service provided by a government or public-sector organisation.
///
/// <para>
/// <b>Required properties (Schema.org + Google Structured Data):</b>
/// <see cref="SchemaOrgThing.Name"/> — always set this for rich-result eligibility.
/// </para>
/// </summary>
/// <remarks>
/// <see cref="SchemaOrgGovernmentService"/> extends <see cref="SchemaOrgService"/>
/// and inherits all its properties.
/// Reference: <see href="https://schema.org/GovernmentService"/>
/// </remarks>
public record SchemaOrgGovernmentService : SchemaOrgService
{
    /// <summary>
    /// The government organisation or department that operates this service.
    /// Typically an <c>@id</c> reference to an <see cref="SchemaOrgOrganization"/> node.
    /// </summary>
    [JsonPropertyName("serviceOperator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ServiceOperator { get; init; }

    /// <summary>
    /// The jurisdiction (country, region, or local authority) in which this
    /// government service applies.
    /// </summary>
    [JsonPropertyName("jurisdiction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Jurisdiction { get; init; }
}
