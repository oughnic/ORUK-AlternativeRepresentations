using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrukModels.SchemaOrg;

/// <summary>
/// Pre-configured <see cref="JsonSerializerOptions"/> for serialising Schema.org
/// JSON-LD output.
///
/// Key settings:
/// <list type="bullet">
///   <item>Null properties are omitted (<c>WhenWritingNull</c>).</item>
///   <item>The encoder permits non-ASCII characters in URI values.</item>
///   <item>Property names are written exactly as declared (no automatic camelCase).</item>
/// </list>
/// </summary>
public static class SchemaOrgSerializerOptions
{
    /// <summary>
    /// The recommended <see cref="JsonSerializerOptions"/> for producing Schema.org
    /// JSON-LD output.  Use this when calling <see cref="JsonSerializer.Serialize"/>.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
