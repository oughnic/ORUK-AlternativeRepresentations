using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace OrukApiClient;

/// <summary>
/// Geocodes UK postcodes using the free, unauthenticated
/// <a href="https://postcodes.io">postcodes.io</a> API.
/// </summary>
/// <remarks>
/// Only strings that match a UK postcode pattern are sent to the API.
/// Place names and other non-postcode strings return <see langword="null"/>
/// without making a network call.  All failures are logged as warnings and
/// return <see langword="null"/> — geocoding failures are non-fatal.
/// </remarks>
public sealed class PostcodesIoGeocoder : IPostcodeGeocoder
{
    // Matches full UK postcodes with optional space: "DN15 7BH", "SW1A2AA", etc.
    private static readonly Regex UkPostcodeRegex = new(
        @"^[A-Z]{1,2}[0-9][0-9A-Z]?\s?[0-9][A-Z]{2}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly ILogger<PostcodesIoGeocoder> _logger;

    public PostcodesIoGeocoder(HttpClient httpClient, ILogger<PostcodesIoGeocoder> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<GeoPoint?> LookupAsync(
        string postcodeOrLocation,
        CancellationToken cancellationToken = default)
    {
        var trimmed = postcodeOrLocation.Trim();

        if (!UkPostcodeRegex.IsMatch(trimmed))
        {
            _logger.LogDebug(
                "'{Input}' does not match a UK postcode pattern — skipping geocoding.",
                trimmed);
            return null;
        }

        // Normalise: remove spaces so the URL path is clean.
        var encoded = Uri.EscapeDataString(trimmed.Replace(" ", "").ToUpperInvariant());
        var url = $"https://api.postcodes.io/postcodes/{encoded}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "postcodes.io returned HTTP {Code} for postcode '{Postcode}'. " +
                    "Location filtering will be omitted.",
                    (int)response.StatusCode, trimmed);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);

            if (!doc.RootElement.TryGetProperty("result", out var result))
            {
                _logger.LogWarning(
                    "postcodes.io response for '{Postcode}' had no 'result' field.",
                    trimmed);
                return null;
            }

            if (!result.TryGetProperty("latitude", out var latEl) ||
                !result.TryGetProperty("longitude", out var lonEl))
            {
                _logger.LogWarning(
                    "postcodes.io result for '{Postcode}' missing latitude/longitude.",
                    trimmed);
                return null;
            }

            var lat = latEl.GetDouble();
            var lon = lonEl.GetDouble();

            _logger.LogInformation(
                "Geocoded '{Postcode}' → ({Lat}, {Lon}).",
                trimmed, lat, lon);

            return new GeoPoint(lat, lon);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex,
                "Geocoding failed for '{Postcode}'. Location filtering will be omitted.",
                trimmed);
            return null;
        }
    }
}
