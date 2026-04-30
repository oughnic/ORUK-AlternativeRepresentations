namespace OrukTransformer.Core.Vodim;

/// <summary>
/// VODIM data-quality classification applied to each field in the ORUK → Schema.org
/// transformation pipeline.
///
/// Each field that <em>could potentially be mapped</em> receives exactly one
/// classification, based on both the source ORUK specification and the target
/// Schema.org specification.
///
/// <list type="table">
///   <listheader>
///     <term>Code</term><description>Meaning</description>
///   </listheader>
///   <item>
///     <term>V — Valid</term>
///     <description>
///       The source value is present, satisfies the ORUK field constraints,
///       and the derived Schema.org value satisfies the target property constraints.
///       The mapping is clean and complete.
///     </description>
///   </item>
///   <item>
///     <term>O — Other</term>
///     <description>
///       The source value is present but falls outside the expected controlled
///       vocabulary or enumeration for <em>either</em> the ORUK source or the
///       Schema.org target (e.g. an unrecognised ORUK <c>status</c> value, or
///       a day-of-week written in long-form rather than RRULE short-form).
///       The value was carried through, but its semantic validity cannot be
///       guaranteed.
///     </description>
///   </item>
///   <item>
///     <term>D — Default</term>
///     <description>
///       The source value was absent (null or empty) but a default was applied
///       so that the target property could still be emitted with a valid value
///       (e.g. currency defaulted to "GBP").
///     </description>
///   </item>
///   <item>
///     <term>I — Invalid</term>
///     <description>
///       The source value is present but fails a concrete format or range
///       validation rule (e.g. an unparseable date string, a negative age,
///       a latitude outside −90..90).  The field was <em>not</em> mapped to
///       the target; the output property was omitted.
///     </description>
///   </item>
///   <item>
///     <term>M — Missing</term>
///     <description>
///       The source value is absent (null or empty string) and no default was
///       applied.  The corresponding target property was omitted from the output.
///     </description>
///   </item>
///   <item>
///     <term>U — Unmapped</term>
///     <description>
///       The source field has <em>no Schema.org mapping</em>
///       (target path shown as <c>—</c>).  The value is present but is
///       intentionally omitted from the output because no suitable Schema.org
///       property exists.  This is a known lossy transformation; see
///       <em>README.md § Unmapped ORUK Fields</em> for the full list.
///     </description>
///   </item>
/// </list>
/// </summary>
public enum VodimClassification
{
    /// <summary>Value present, valid in ORUK source, maps cleanly to valid Schema.org value.</summary>
    Valid,

    /// <summary>Value present but outside expected controlled vocabulary for source or target.</summary>
    Other,

    /// <summary>Source value absent; a default value was applied and emitted to the target.</summary>
    Default,

    /// <summary>Value present but fails format/range validation; target property was omitted.</summary>
    Invalid,

    /// <summary>Value absent in source; no default applied; target property was omitted.</summary>
    Missing,

    /// <summary>
    /// Value present in source but no Schema.org mapping exists; field is intentionally
    /// omitted from output (lossy transformation).  Target path is shown as <c>—</c>.
    /// </summary>
    Unmapped
}
