using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrukModels.Models;

/// <summary>
/// Represents a single non-standard (vendor-extended) field captured from an ORUK feed
/// that does not correspond to any defined HSDS / ORUK v3 property.
/// </summary>
/// <remarks>
/// <para>
/// ORUK feeds may include proprietary extensions added by the publishing platform
/// (e.g. PlaceCube's <c>pc_</c>-prefixed fields in the Bristol Open Place Directory).
/// Rather than modelling each vendor's fields as bespoke C# types, all unrecognised
/// JSON properties are captured generically via <see cref="JsonExtensionDataAttribute"/>
/// and surfaced as <see cref="OrukExtensionProperty"/> instances.
/// </para>
/// <para>
/// The <see cref="Namespace"/> is derived from the JSON field name by splitting on the
/// first underscore (<c>_</c>).  For example, the field <c>pc_metadata</c> yields
/// <c>Namespace = "pc"</c> and <c>Key = "metadata"</c>.  If the field name contains no
/// underscore the <see cref="Namespace"/> is an empty string and <see cref="Key"/> is
/// the full field name.
/// </para>
/// <para>
/// The raw JSON value is preserved as a <see cref="JsonElement"/>, which supports
/// scalars, objects, and arrays without loss of fidelity.
/// </para>
/// </remarks>
public record OrukExtensionProperty
{
    /// <summary>
    /// The vendor/platform namespace prefix extracted from the JSON field name
    /// (everything before the first <c>_</c>).
    /// For example, <c>"pc"</c> from <c>pc_metadata</c>.
    /// An empty string indicates no namespace prefix was present.
    /// </summary>
    public string Namespace { get; init; } = string.Empty;

    /// <summary>
    /// The extension key (everything after the first <c>_</c> in the JSON field name).
    /// For example, <c>"metadata"</c> from <c>pc_metadata</c>.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// The original JSON field name exactly as it appeared in the feed,
    /// e.g. <c>"pc_metadata"</c> or <c>"pc_targetAudience"</c>.
    /// </summary>
    public string RawKey { get; init; } = string.Empty;

    /// <summary>
    /// The raw JSON value for this extension field.
    /// May be a string, number, boolean, object, array, or null.
    /// Use the <see cref="JsonElement"/> API to inspect or extract values.
    /// </summary>
    public JsonElement Value { get; init; }

    /// <summary>
    /// Parses a raw JSON field name into an <see cref="OrukExtensionProperty"/>.
    /// The namespace is extracted by splitting <paramref name="rawKey"/> on the
    /// first underscore.
    /// </summary>
    /// <param name="rawKey">The JSON field name (e.g. <c>"pc_metadata"</c>).</param>
    /// <param name="value">The deserialized JSON value.</param>
    internal static OrukExtensionProperty FromRawKey(string rawKey, JsonElement value)
    {
        var separatorIndex = rawKey.IndexOf('_');
        string ns, key;

        if (separatorIndex > 0)
        {
            ns = rawKey[..separatorIndex];
            key = rawKey[(separatorIndex + 1)..];
        }
        else
        {
            ns = string.Empty;
            key = rawKey;
        }

        return new OrukExtensionProperty
        {
            Namespace = ns,
            Key = key,
            RawKey = rawKey,
            Value = value
        };
    }
}
