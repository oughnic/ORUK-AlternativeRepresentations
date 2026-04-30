using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A contact point for a person, organisation, or service (phone, email, web form, etc.).
/// Maps from an ORUK <c>Contact</c> entity.
///
/// <para>
/// <b>Required properties (Google Structured Data):</b>
/// <see cref="ContactType"/> — use a descriptive value such as
/// <c>"customer support"</c>, <c>"enquiries"</c>, or <c>"referrals"</c>.
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/ContactPoint"/>
/// </remarks>
public record SchemaOrgContactPoint
{
    /// <summary>Schema.org type discriminator — always <c>"ContactPoint"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "ContactPoint";

    /// <summary>
    /// A description of what type of contact is being provided.
    /// Maps from ORUK <c>Contact.name</c> or <c>Contact.department</c>.
    /// <b>Required</b> by Google for rich results.
    /// Examples: <c>"customer support"</c>, <c>"enquiries"</c>, <c>"referrals"</c>.
    /// </summary>
    [JsonPropertyName("contactType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContactType { get; init; }

    /// <summary>
    /// The telephone number, including country code where possible
    /// (e.g. <c>"+44 117 123 4567"</c>).
    /// Maps from ORUK <c>Phone.number</c>.
    /// </summary>
    [JsonPropertyName("telephone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Telephone { get; init; }

    /// <summary>Email address for the contact point.</summary>
    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; init; }

    /// <summary>URL of the contact page or web form.</summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; init; }

    /// <summary>
    /// A human-readable name for this contact (e.g. a person's name or job title).
    /// Maps from ORUK <c>Contact.title</c>.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    /// <summary>
    /// Languages in which the contact is available.
    /// </summary>
    [JsonPropertyName("availableLanguage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgLanguage>? AvailableLanguage { get; init; }
}
