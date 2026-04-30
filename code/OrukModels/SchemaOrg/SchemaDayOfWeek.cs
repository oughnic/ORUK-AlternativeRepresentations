namespace OrukModels.SchemaOrg;

/// <summary>
/// Schema.org DayOfWeek enumeration — URI constants for use with
/// <see cref="SchemaOrgOpeningHoursSpecification.DayOfWeek"/>.
///
/// Schema.org defines DayOfWeek as a mandatory vocabulary: only these
/// canonical URI values are valid. Free-text day names must not be used.
/// </summary>
/// <remarks>
/// Reference: <see href="https://schema.org/DayOfWeek"/>
/// </remarks>
public static class SchemaDayOfWeek
{
    /// <summary>Monday — <c>https://schema.org/Monday</c></summary>
    public const string Monday = "https://schema.org/Monday";

    /// <summary>Tuesday — <c>https://schema.org/Tuesday</c></summary>
    public const string Tuesday = "https://schema.org/Tuesday";

    /// <summary>Wednesday — <c>https://schema.org/Wednesday</c></summary>
    public const string Wednesday = "https://schema.org/Wednesday";

    /// <summary>Thursday — <c>https://schema.org/Thursday</c></summary>
    public const string Thursday = "https://schema.org/Thursday";

    /// <summary>Friday — <c>https://schema.org/Friday</c></summary>
    public const string Friday = "https://schema.org/Friday";

    /// <summary>Saturday — <c>https://schema.org/Saturday</c></summary>
    public const string Saturday = "https://schema.org/Saturday";

    /// <summary>Sunday — <c>https://schema.org/Sunday</c></summary>
    public const string Sunday = "https://schema.org/Sunday";

    /// <summary>
    /// Public holidays — <c>https://schema.org/PublicHolidays</c>.
    /// Use to describe opening hours that apply on all public holidays.
    /// </summary>
    public const string PublicHolidays = "https://schema.org/PublicHolidays";

    /// <summary>
    /// Returns all valid day-of-week URI values.
    /// </summary>
    public static IReadOnlyList<string> AllValues { get; } =
    [
        Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday, PublicHolidays
    ];
}
