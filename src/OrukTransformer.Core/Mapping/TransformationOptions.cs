namespace OrukTransformer.Core.Mapping;

/// <summary>
/// Configuration options for the ORUK → Schema.org transformation pipeline.
/// Injected into <see cref="IOrukToSchemaOrgTransformer.Transform"/> to provide
/// deployment-specific settings without hardcoding.
/// </summary>
public record TransformationOptions
{
    /// <summary>
    /// Base URL of this deployment, used to construct stable <c>@id</c> URIs
    /// for every node in the output <c>@graph</c>.
    ///
    /// Example: <c>"https://services.bristol.gov.uk"</c>
    ///
    /// Must not end with a trailing slash.
    /// </summary>
    public string BaseUrl { get; init; } = "https://services.example.org";

    /// <summary>
    /// ISO 4217 currency code to use when an ORUK <c>CostOption</c> does not
    /// specify a currency. Defaults to <c>"GBP"</c>.
    /// </summary>
    public string DefaultCurrency { get; init; } = "GBP";

    // ── URI construction helpers ─────────────────────────────────────────────────

    /// <summary>Returns the canonical <c>@id</c> URI for an ORUK service.</summary>
    public string ServiceUri(string id) => $"{BaseUrl}/services/{id}";

    /// <summary>Returns the canonical <c>@id</c> URI for an ORUK organisation.</summary>
    public string OrganisationUri(string id) => $"{BaseUrl}/organisations/{id}";

    /// <summary>Returns the canonical <c>@id</c> URI for an ORUK location.</summary>
    public string LocationUri(string id) => $"{BaseUrl}/locations/{id}";
}
