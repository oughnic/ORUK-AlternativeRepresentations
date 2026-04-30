using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS CostOption — pricing or fee information for a service.
/// </summary>
public class OrukCostOption
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("valid_from")]
    public string? ValidFrom { get; set; }

    [JsonPropertyName("valid_to")]
    public string? ValidTo { get; set; }

    /// <summary>
    /// Free-text description of the cost option (e.g. "Adult", "Concession", "Free").
    /// </summary>
    [JsonPropertyName("option")]
    public string? Option { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("amount_description")]
    public string? AmountDescription { get; set; }

    /// <summary>ISO 4217 currency code. Defaults to "GBP".</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
