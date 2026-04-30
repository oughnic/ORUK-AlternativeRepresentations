using System.Text.Json;
using OrukModels.SchemaOrg;

namespace OrukTransformer.Cli.Output;

/// <summary>
/// Serialises a <see cref="SchemaOrgDocument"/> to a file or to stdout.
/// Uses <see cref="SchemaOrgSerializerOptions.Default"/> for all serialisation.
/// </summary>
public sealed class JsonLdWriter : IJsonLdWriter
{
    private static readonly byte[] Newline = [(byte)'\n'];

    /// <inheritdoc/>
    public async Task WriteAsync(
        SchemaOrgDocument document,
        FileInfo? outputFile,
        CancellationToken cancellationToken = default)
    {
        if (outputFile is not null)
        {
            await using var fileStream = new FileStream(
                outputFile.FullName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 65536,
                useAsync: true);

            await JsonSerializer.SerializeAsync(
                fileStream,
                document,
                SchemaOrgSerializerOptions.Default,
                cancellationToken);
        }
        else
        {
            // Write to stdout — use a StreamWriter wrapping the stdout stream
            // so we can serialise directly without building an intermediate string
            await using var stdoutStream = Console.OpenStandardOutput();
            await JsonSerializer.SerializeAsync(
                stdoutStream,
                document,
                SchemaOrgSerializerOptions.Default,
                cancellationToken);

            // Newline after the JSON so the terminal prompt appears on a fresh line
            await stdoutStream.WriteAsync(Newline, cancellationToken);
        }
    }
}
