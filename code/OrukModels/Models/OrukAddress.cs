using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Address record associated with a location.
/// </summary>
public class OrukAddress
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    [JsonPropertyName("attention")]
    public string? Attention { get; set; }

    [JsonPropertyName("address_1")]
    public string? Address1 { get; set; }

    [JsonPropertyName("address_2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("state_province")]
    public string? StateProvince { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code (e.g. "GB").</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>Type of address: "physical" or "postal".</summary>
    [JsonPropertyName("address_type")]
    public string? AddressType { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
