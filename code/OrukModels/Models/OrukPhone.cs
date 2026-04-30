using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Phone number associated with a service, organisation,
/// location, contact, or service-at-location record.
/// </summary>
public class OrukPhone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonPropertyName("contact_id")]
    public string? ContactId { get; set; }

    [JsonPropertyName("service_at_location_id")]
    public string? ServiceAtLocationId { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("extension")]
    public double? Extension { get; set; }

    /// <summary>
    /// Phone type drawn from RFC 6350: text, voice, fax, cell, video, pager, textphone.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("languages")]
    public virtual ICollection<OrukLanguage> Languages { get; set; } = [];

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
