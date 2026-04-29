using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS ServiceArea describing the geographic coverage
/// of a service, optionally including an ONS geography code.
/// </summary>
public class OrukServiceArea
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    /// <summary>
    /// Bristol OPD uses "serviceId" (camelCase) rather than "service_id".
    /// Both are captured to support liberal receive semantics.
    /// </summary>
    [JsonPropertyName("serviceId")]
    public string? ServiceIdCamel { get; set; }

    /// <summary>Human-readable name of the service area (e.g. "Bristol").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// ONS geographic code (LAD, LSOA, MSOA, etc.) or other geographic identifier.
    /// </summary>
    [JsonPropertyName("extent")]
    public string? Extent { get; set; }

    /// <summary>
    /// Type of geographic extent (e.g. "LocalAuthorityDistrict", "LSOA").
    /// </summary>
    [JsonPropertyName("extent_type")]
    public string? ExtentType { get; set; }

    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
