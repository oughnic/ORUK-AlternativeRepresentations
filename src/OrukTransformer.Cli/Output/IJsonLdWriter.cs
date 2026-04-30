using OrukModels.SchemaOrg;

namespace OrukTransformer.Cli.Output;

/// <summary>
/// Writes a <see cref="SchemaOrgDocument"/> as JSON-LD to a file or to
/// standard output.
/// </summary>
public interface IJsonLdWriter
{
    /// <summary>
    /// Serialises <paramref name="document"/> and writes it to
    /// <paramref name="outputFile"/>, or to <see cref="Console.Out"/> when
    /// <paramref name="outputFile"/> is <c>null</c>.
    /// </summary>
    Task WriteAsync(SchemaOrgDocument document, FileInfo? outputFile,
        CancellationToken cancellationToken = default);
}
