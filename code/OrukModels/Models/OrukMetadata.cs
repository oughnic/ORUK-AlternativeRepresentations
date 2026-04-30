using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Metadata record — an audit trail entry
/// recording changes made to data in the feed.
/// </summary>
public class OrukMetadata
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("resource_id")]
    public string? ResourceId { get; set; }

    [JsonPropertyName("resource_type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("last_action_date")]
    public string? LastActionDate { get; set; }

    [JsonPropertyName("last_action_type")]
    public string? LastActionType { get; set; }

    [JsonPropertyName("field_name")]
    public string? FieldName { get; set; }

    [JsonPropertyName("previous_value")]
    public string? PreviousValue { get; set; }

    [JsonPropertyName("replacement_value")]
    public string? ReplacementValue { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }
}
