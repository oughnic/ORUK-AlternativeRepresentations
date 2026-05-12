namespace OrukApiClient;

/// <summary>
/// Converts a UK postcode string into WGS84 coordinates suitable for passing
/// to the ORUK <c>proximity</c> query parameter.
/// </summary>
/// <remarks>
/// ORUK v3 requires <c>proximity</c> in <c>latitude,longitude</c> format.
/// Passing a raw postcode (e.g. "DN15 7BH") to the endpoint causes zero results.
/// Callers should call <see cref="LookupAsync"/> and use the resolved coordinates
/// when present, or omit the proximity parameter when geocoding fails.
/// </remarks>
public interface IPostcodeGeocoder
{
    /// <summary>
    /// Returns WGS84 coordinates for the given UK postcode, or
    /// <see langword="null"/> when the input is not a recognisable UK postcode
    /// or the lookup fails.
    /// </summary>
    Task<GeoPoint?> LookupAsync(string postcodeOrLocation, CancellationToken cancellationToken = default);
}

/// <summary>WGS84 latitude/longitude pair.</summary>
public record GeoPoint(double Latitude, double Longitude)
{
    /// <summary>
    /// Returns the coordinate as a comma-separated string suitable for the ORUK
    /// <c>proximity</c> query parameter.
    /// </summary>
    public override string ToString() =>
        $"{Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
        $"{Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
}
