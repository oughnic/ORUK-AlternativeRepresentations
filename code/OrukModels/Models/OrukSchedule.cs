using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents an ORUK v3 / HSDS Schedule defining when a service or location is available.
/// Supports both simple open/close time patterns and RFC 5545 (iCalendar) recurrence rules.
/// </summary>
public class OrukSchedule
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    [JsonPropertyName("service_at_location_id")]
    public string? ServiceAtLocationId { get; set; }

    [JsonPropertyName("valid_from")]
    public string? ValidFrom { get; set; }

    [JsonPropertyName("valid_to")]
    public string? ValidTo { get; set; }

    // ── iCalendar / RFC 5545 recurrence fields ───────────────────────────────────

    [JsonPropertyName("dtstart")]
    public string? DtStart { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("until")]
    public string? Until { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("wkst")]
    public string? Wkst { get; set; }

    /// <summary>
    /// Recurrence frequency: WEEKLY, DAILY, MONTHLY, etc.
    /// </summary>
    [JsonPropertyName("freq")]
    public string? Freq { get; set; }

    [JsonPropertyName("interval")]
    public int? Interval { get; set; }

    /// <summary>Days of the week (e.g. "MO,WE,FR").</summary>
    [JsonPropertyName("byday")]
    public string? ByDay { get; set; }

    [JsonPropertyName("bymonthday")]
    public string? ByMonthDay { get; set; }

    // ── Simple opening-hours fields ──────────────────────────────────────────────

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("opens_at")]
    public string? OpensAt { get; set; }

    [JsonPropertyName("closes_at")]
    public string? ClosesAt { get; set; }

    [JsonPropertyName("schedule_link")]
    public string? ScheduleLink { get; set; }

    [JsonPropertyName("attending_type")]
    public string? AttendingType { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("attributes")]
    public virtual ICollection<OrukAttribute> Attributes { get; set; } = [];

    [JsonPropertyName("metadata")]
    public virtual ICollection<OrukMetadata> Metadata { get; set; } = [];
}
