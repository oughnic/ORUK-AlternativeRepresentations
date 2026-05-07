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
    internal static Uri BuildServicesUrl(Uri feedBaseUrl, OrukServiceQuery query, int page, int perPage)
    {
        var baseServices = new Uri(EnsureBase(feedBaseUrl) + "/services");
        var builder = new UriBuilder(baseServices);
        var qs = System.Web.HttpUtility.ParseQueryString(builder.Query);

        qs["page"] = page.ToString();
        qs["per_page"] = perPage.ToString();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            qs["text"] = query.Keyword;

        if (!string.IsNullOrWhiteSpace(query.Proximity))
            qs["proximity"] = query.Proximity;

        if (query.RadiusKm.HasValue)
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
