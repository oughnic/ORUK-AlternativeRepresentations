using OrukModels.SchemaOrg;
using OrukTransformer.Core.Vodim;

namespace OrukTransformer.Core.Mapping;

/// <summary>
/// The result of a single ORUK → Schema.org transformation.
///
/// <list type="bullet">
///   <item>
///     <see cref="Document"/> — the JSON-LD <c>@graph</c> document ready for serialisation.
///   </item>
///   <item>
///     <see cref="Report"/> — the field-by-field VODIM quality report produced
///     during the transformation.
///   </item>
/// </list>
/// </summary>
public record TransformationResult
{
    /// <summary>
    /// The Schema.org JSON-LD document produced from the ORUK service.
    /// Serialise with <see cref="SchemaOrgSerializerOptions.Default"/>.
    /// </summary>
    public required SchemaOrgDocument Document { get; init; }

    /// <summary>
    /// Field-by-field VODIM quality report for this transformation.
    /// Contains one <see cref="FieldMappingRecord"/> per ORUK field that
    /// could potentially be mapped.
    /// </summary>
    public required TransformationReport Report { get; init; }
}
