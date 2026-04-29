using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Non-standard "pc_metadata" extension present in the Bristol Open Place Directory (OPD) feed.
/// These fields are preserved during ingestion to avoid data loss but are not emitted in output.
/// </summary>
public record OrukPcMetadata
{
    [JsonPropertyName("assured_by")]
    public string? AssuredBy { get; init; }

    [JsonPropertyName("date_assured")]
    public string? DateAssured { get; init; }

    [JsonPropertyName("date_created")]
    public string? DateCreated { get; init; }

    [JsonPropertyName("date_modified")]
    public string? DateModified { get; init; }
}

/// <summary>
/// Non-standard "pc_targetAudience" extension present in the Bristol Open Place Directory feed.
/// Represents an audience type tag applied to a service.
/// These are preserved during ingestion but are not emitted in standard output.
/// </summary>
public record OrukPcTargetAudience
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("audienceType")]
    public string? AudienceType { get; init; }
}
