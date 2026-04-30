using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// Intended audience of the service — who the service is for.
/// Maps from an ORUK <c>Eligibility</c> entity or the service-level
/// <c>eligibility_description</c> field.
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/Audience"/>
/// </remarks>
public record SchemaOrgAudience
{
    /// <summary>Schema.org type discriminator — <c>"Audience"</c> or <c>"PeopleAudience"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "Audience";

    /// <summary>
    /// The category of audience (e.g. <c>"Adults"</c>, <c>"Young people"</c>,
    /// <c>"Carers"</c>).  Maps from ORUK <c>Eligibility.eligibility</c> or
    /// the service's <c>eligibility_description</c>.
    /// </summary>
    [JsonPropertyName("audienceType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AudienceType { get; init; }

    /// <summary>
    /// A description of the intended audience.
    /// Maps from ORUK <c>eligibility_description</c>.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
}

/// <summary>
/// A group of people with a common characteristic related to age.
/// Use when the service has minimum or maximum age eligibility criteria.
/// Maps from ORUK <c>Service.minimum_age</c> / <c>maximum_age</c>.
/// </summary>
/// <remarks>
/// <see cref="SchemaOrgPeopleAudience"/> extends <see cref="SchemaOrgAudience"/>.
/// Reference: <see href="https://schema.org/PeopleAudience"/>
/// </remarks>
public record SchemaOrgPeopleAudience : SchemaOrgAudience
{
    /// <summary>Schema.org type discriminator — always <c>"PeopleAudience"</c>.</summary>
    [JsonPropertyName("@type")]
    public new string Type { get; } = "PeopleAudience";

    /// <summary>
    /// Minimum recommended age for the audience.
    /// Maps from ORUK <c>Service.minimum_age</c> or <c>Eligibility.minimum_age</c>.
    /// </summary>
    [JsonPropertyName("suggestedMinAge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SuggestedMinAge { get; init; }

    /// <summary>
    /// Maximum recommended age for the audience.
    /// Maps from ORUK <c>Service.maximum_age</c> or <c>Eligibility.maximum_age</c>.
    /// </summary>
    [JsonPropertyName("suggestedMaxAge")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? SuggestedMaxAge { get; init; }
}
