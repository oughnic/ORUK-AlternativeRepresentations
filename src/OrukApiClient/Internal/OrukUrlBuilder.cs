using System.Globalization;

namespace OrukApiClient.Internal;

/// <summary>
/// Builds ORUK v3 API endpoint URLs from a base URL and query parameters.
/// </summary>
internal static class OrukUrlBuilder
{
    /// <summary>
    /// Returns the /services list URL, appending pagination and keyword parameters
    /// derived from the given query.
    /// </summary>
    /// <param name="feedBaseUrl">Base URL of the ORUK feed.</param>
    /// <param name="query">Query parameters.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="perPage">Page size.</param>
    /// <param name="resolvedProximity">
    /// Pre-geocoded <c>latitude,longitude</c> string to pass as the <c>proximity</c>
    /// parameter.  When <see langword="null"/> the proximity is omitted, even if
    /// <see cref="OrukServiceQuery.Proximity"/> is set (because the raw postcode
    /// string is not a valid value for the ORUK <c>proximity</c> parameter).
    /// </param>
    internal static Uri BuildServicesUrl(
        Uri feedBaseUrl,
        OrukServiceQuery query,
        int page,
        int perPage,
        string? resolvedProximity = null)
    {
        var baseServices = new Uri(EnsureBase(feedBaseUrl) + "/services");
        var builder = new UriBuilder(baseServices);
        var qs = System.Web.HttpUtility.ParseQueryString(builder.Query);

        qs["page"] = page.ToString();
        qs["per_page"] = perPage.ToString();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            qs["text"] = query.Keyword;

        if (!string.IsNullOrWhiteSpace(resolvedProximity))
            qs["proximity"] = resolvedProximity;

        if (query.RadiusKm.HasValue && !string.IsNullOrWhiteSpace(resolvedProximity))
            qs["radius"] = query.RadiusKm.Value.ToString("F1", CultureInfo.InvariantCulture);

        builder.Query = qs.ToString();
        return builder.Uri;
    }

    /// <summary>Returns the /services/{id} URL for a single-record fetch.</summary>
    internal static Uri BuildServiceByIdUrl(Uri feedBaseUrl, string serviceId)
    {
        var encoded = Uri.EscapeDataString(serviceId);
        return new Uri(EnsureBase(feedBaseUrl) + $"/services/{encoded}");
    }

    /// <summary>Returns the paginated /organizations URL with optional keyword filter.</summary>
    internal static Uri BuildOrganizationsUrl(Uri feedBaseUrl, string? keyword, int page, int perPage)
    {
        var base_ = new Uri(EnsureBase(feedBaseUrl) + "/organizations");
        var builder = new UriBuilder(base_);
        var qs = System.Web.HttpUtility.ParseQueryString(builder.Query);
        qs["page"] = page.ToString();
        qs["per_page"] = perPage.ToString();
        if (!string.IsNullOrWhiteSpace(keyword))
            qs["text"] = keyword;
        builder.Query = qs.ToString();
        return builder.Uri;
    }

    /// <summary>Returns the /organizations/{id} URL for a single-record fetch.</summary>
    internal static Uri BuildOrganizationByIdUrl(Uri feedBaseUrl, string organizationId)
    {
        var encoded = Uri.EscapeDataString(organizationId);
        return new Uri(EnsureBase(feedBaseUrl) + $"/organizations/{encoded}");
    }

    /// <summary>Returns the paginated /taxonomy_terms URL.</summary>
    internal static Uri BuildTaxonomyTermsUrl(Uri feedBaseUrl, int page, int perPage)
    {
        var base_ = new Uri(EnsureBase(feedBaseUrl) + "/taxonomy_terms");
        var builder = new UriBuilder(base_);
        var qs = System.Web.HttpUtility.ParseQueryString(builder.Query);
        qs["page"] = page.ToString();
        qs["per_page"] = perPage.ToString();
        builder.Query = qs.ToString();
        return builder.Uri;
    }

    /// <summary>
    /// Returns the canonical base URL — no trailing slash, and strips a trailing
    /// /services segment if the caller passed the services endpoint URL directly.
    /// </summary>
    internal static string EnsureBase(Uri uri)
    {
        var s = uri.ToString().TrimEnd('/');
        if (s.EndsWith("/services", StringComparison.OrdinalIgnoreCase))
            s = s[..^"/services".Length];
        return s;
    }
}
