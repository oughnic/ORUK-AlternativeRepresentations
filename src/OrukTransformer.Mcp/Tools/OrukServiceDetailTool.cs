using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OrukApiClient;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool for retrieving the full details of a specific ORUK service.
/// The AI agent calls this after <see cref="OrukServiceSearchTool.SearchServices"/>
/// to get complete information about a service the user is interested in.
/// </summary>
[McpServerToolType]
public sealed class OrukServiceDetailTool(
    IOrukServiceClient serviceClient,
    ILogger<OrukServiceDetailTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Get the full details of a specific service from an Open Referral UK directory, " +
        "including contact information, opening times, eligibility criteria, cost, " +
        "and accessibility information. Use the feed_url and service_id values returned " +
        "by search_services.")]
    public async Task<string> GetServiceDetail(
        [Description("The base URL of the ORUK feed the service was found in (returned by search_services).")]
        string feedUrl,
        [Description("The unique service ID (returned by search_services).")]
        string serviceId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GetServiceDetail: service={ServiceId}, feed={FeedUrl}.", serviceId, feedUrl);

        if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
        {
            logger.LogWarning("GetServiceDetail: invalid feed_url value '{FeedUrl}'.", feedUrl);
            return """{"error":"Invalid feed_url value."}""";
        }

        var service = await serviceClient.GetByIdAsync(feedUri, serviceId, cancellationToken);

        if (service is null)
        {
            logger.LogWarning(
                "GetServiceDetail: service {ServiceId} not found in feed {FeedUrl}.",
                serviceId, feedUrl);
            return $"{{\"error\":\"Service '{serviceId}' was not found in feed '{feedUrl}'.\"}}";
        }

        logger.LogInformation(
            "GetServiceDetail: retrieved '{ServiceName}' (id={ServiceId}) from {FeedUrl}.",
            service.Name, service.Id, feedUrl);

        // Opening hours — flatten schedules from service and service_at_locations
        var allSchedules = service.Schedules
            .Concat(service.ServiceAtLocations.SelectMany(sal => sal.Schedules))
            .ToList();

        var openingTimes = allSchedules
            .Where(s => s.Description is not null || s.OpensAt is not null)
            .Select(s => new
            {
                description = s.Description,
                days = s.ByDay,
                opens = s.OpensAt,
                closes = s.ClosesAt,
                valid_from = s.ValidFrom,
                valid_to = s.ValidTo
            })
            .ToList();

        // Accessibility — from service_at_locations → location → accessibility
        var accessibility = service.ServiceAtLocations
            .Select(sal => sal.Location)
            .Where(l => l is not null)
            .SelectMany(l => l!.Accessibility)
            .Select(a => a.Description)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .ToList();

        // Locations
        var locations = service.ServiceAtLocations
            .Select(sal => sal.Location)
            .Where(l => l is not null)
            .Select(l =>
            {
                var addr = l!.PhysicalAddresses.FirstOrDefault();
                return new
                {
                    name = l.Name,
                    address = addr is null ? null : new
                    {
                        line1 = addr.Address1,
                        city = addr.City,
                        county = addr.StateProvince,
                        postcode = addr.PostalCode,
                        country = addr.Country
                    },
                    latitude = l.Latitude,
                    longitude = l.Longitude
                };
            })
            .ToList();

        // Eligibility
        var eligibility = service.Eligibility
            .Select(e => e.Description)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .ToList();

        // Cost
        var cost = service.CostOptions
            .Select(c => new
            {
                option = c.Option,
                amount = c.Amount,
                currency = c.Currency,
                description = c.AmountDescription
            })
            .ToList();

        // Languages
        var languages = service.Languages
            .Select(l => l.Name ?? l.Code)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var detail = new
        {
            id = service.Id,
            feed_url = feedUrl,
            name = service.Name,
            alternate_name = service.AlternateName,
            description = service.Description,
            status = service.Status,
            url = service.Url,
            email = service.Email,
            alert = service.Alert,
            organization = service.Organization is null ? null : new
            {
                name = service.Organization.Name,
                description = service.Organization.Description,
                email = service.Organization.Email,
                url = service.Organization.Url
            },
            phones = service.Phones.Select(p => new { p.Number, p.Type }).ToList(),
            locations,
            opening_times = openingTimes.Count > 0 ? openingTimes : null,
            eligibility = eligibility.Count > 0 ? eligibility : null,
            cost = cost.Count > 0 ? cost : null,
            fees_description = service.FeesDescription,
            languages = languages.Count > 0 ? languages : null,
            accessibility = accessibility.Count > 0 ? accessibility : null,
            application_process = service.ApplicationProcess,
            interpretation_services = service.InterpretationServices,
            minimum_age = service.MinimumAge,
            maximum_age = service.MaximumAge,
            last_modified = service.LastModified
        };

        return JsonSerializer.Serialize(detail, JsonOptions);
    }
}
