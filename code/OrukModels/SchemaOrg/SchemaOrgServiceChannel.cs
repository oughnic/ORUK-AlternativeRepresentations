using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// A means for accessing a service, e.g. a phone banking channel, a web site, a branch,
/// or a telephone call centre.
/// Represents how a service can be accessed (online, by phone, in person, etc.).
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/ServiceChannel"/>
/// </remarks>
public record SchemaOrgServiceChannel
{
    /// <summary>Schema.org type discriminator — always <c>"ServiceChannel"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "ServiceChannel";

    /// <summary>
    /// The website through which the service can be accessed.
    /// Maps from ORUK service or contact <c>url</c>.
    /// </summary>
    [JsonPropertyName("serviceUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServiceUrl { get; init; }

    /// <summary>
    /// The phone number to call to access the service.
    /// </summary>
    [JsonPropertyName("servicePhone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgContactPoint? ServicePhone { get; init; }

    /// <summary>
    /// The physical address where the service can be accessed in person.
    /// </summary>
    [JsonPropertyName("serviceLocation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SchemaOrgPlace? ServiceLocation { get; init; }

    /// <summary>
    /// Estimated processing time for requests through this channel
    /// (ISO 8601 duration, e.g. <c>"PT2H"</c> for two hours).
    /// </summary>
    [JsonPropertyName("processingTime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProcessingTime { get; init; }

    /// <summary>Languages in which this channel is available.</summary>
    [JsonPropertyName("availableLanguage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<SchemaOrgLanguage>? AvailableLanguage { get; init; }
}
