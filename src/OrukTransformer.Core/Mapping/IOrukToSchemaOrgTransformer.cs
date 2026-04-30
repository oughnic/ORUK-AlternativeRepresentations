using OrukModels.Models;

namespace OrukTransformer.Core.Mapping;

/// <summary>
/// Transforms a single ORUK <see cref="OrukService"/> (with its fully-populated
/// navigation graph) into a Schema.org JSON-LD document accompanied by a
/// field-by-field VODIM quality report.
/// </summary>
public interface IOrukToSchemaOrgTransformer
{
    /// <summary>
    /// Transforms the given <paramref name="service"/> into a
    /// <see cref="TransformationResult"/> containing a Schema.org
    /// <c>@graph</c> document and a VODIM quality report.
    ///
    /// <para>
    /// The method never throws for data-quality reasons; it records all issues
    /// in the <see cref="TransformationResult.Report"/> instead.
    /// </para>
    /// </summary>
    /// <param name="service">
    /// A fully-populated ORUK service object, including navigation properties
    /// (<c>Organization</c>, <c>ServiceAtLocations</c>, <c>Schedules</c>, etc.).
    /// </param>
    /// <param name="options">Deployment-specific configuration.</param>
    TransformationResult Transform(OrukService service, TransformationOptions options);
}
