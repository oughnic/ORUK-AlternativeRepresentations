using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// The opening hours of a place or service for a particular set of days and times.
/// Maps from an ORUK <c>Schedule</c> entity.
///
/// <para>
/// <b>Required properties (Schema.org):</b>
/// At least one of the following must be present:
/// <list type="bullet">
///   <item>
///     <see cref="DayOfWeek"/> (with <see cref="Opens"/> and <see cref="Closes"/>)
///     — for recurring weekly schedules.
///   </item>
///   <item>
///     Both <see cref="ValidFrom"/> and <see cref="ValidThrough"/>
///     — for date-range-only specifications.
///   </item>
/// </list>
/// Use <see cref="SchemaDayOfWeek"/> constants for <see cref="DayOfWeek"/> values;
/// free-text day names are not valid Schema.org.
/// </para>
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/OpeningHoursSpecification"/>
/// </remarks>
public record SchemaOrgOpeningHoursSpecification
{
    /// <summary>Schema.org type discriminator — always <c>"OpeningHoursSpecification"</c>.</summary>
    [JsonPropertyName("@type")]
    public string Type { get; } = "OpeningHoursSpecification";

    /// <summary>
    /// The day of the week for which these opening hours apply.
    /// <b>Must</b> use a <see cref="SchemaDayOfWeek"/> constant URI
    /// (e.g. <c>"https://schema.org/Monday"</c>).
    /// When this property is set, <see cref="Opens"/> and <see cref="Closes"/>
    /// should also be set.
    /// </summary>
    [JsonPropertyName("dayOfWeek")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DayOfWeek { get; init; }

    /// <summary>
    /// The time the business opens, in <c>HH:mm</c> format (ISO 8601 time).
    /// Maps from ORUK <c>Schedule.opens_at</c>.
    /// Required when <see cref="DayOfWeek"/> is set.
    /// </summary>
    [JsonPropertyName("opens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Opens { get; init; }

    /// <summary>
    /// The time the business closes, in <c>HH:mm</c> format (ISO 8601 time).
    /// Maps from ORUK <c>Schedule.closes_at</c>.
    /// Required when <see cref="DayOfWeek"/> is set.
    /// </summary>
    [JsonPropertyName("closes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Closes { get; init; }

    /// <summary>
    /// The date from which these opening hours are valid (ISO 8601 date).
    /// Maps from ORUK <c>Schedule.valid_from</c>.
    /// </summary>
    [JsonPropertyName("validFrom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValidFrom { get; init; }

    /// <summary>
    /// The date until which these opening hours are valid (ISO 8601 date).
    /// Maps from ORUK <c>Schedule.valid_to</c>.
    /// </summary>
    [JsonPropertyName("validThrough")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValidThrough { get; init; }

    /// <summary>
    /// A free-text description of this schedule entry.
    /// Maps from ORUK <c>Schedule.description</c>.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
}
