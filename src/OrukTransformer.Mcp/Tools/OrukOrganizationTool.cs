using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using OrukApiClient;
using OrukTransformer.Mcp.Models;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tools for discovering and profiling the organisations that deliver
/// ORUK-listed services. Two tools are provided in one class:
/// <list type="bullet">
///   <item><description><c>search_organisations</c> — keyword search across all configured feeds.</description></item>
///   <item><description><c>get_organisation_detail</c> — full profile for a single organisation by ID.</description></item>
/// </list>
/// </summary>
[McpServerToolType]
public sealed class OrukOrganizationTool(
    IOrukOrganizationClient organizationClient,
    IOrukServiceClient serviceClient,
    IOptions<McpOptions> options,
    IReadOnlyList<Uri> feedUrls,
    ILogger<OrukOrganizationTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Search for the organisations (charities, councils, NHS bodies, community groups) " +
        "that deliver services listed in the Open Referral UK directories. " +
        "Use this when the user wants to find a specific provider rather than a specific service — " +
        "for example 'Which charities support people with dementia in Bristol?', " +
        "'Find organisations that run food banks', or 'Is there an Age UK near me?'. " +
        "Returns organisation name, description, contact details, website, and legal status.")]
    public async Task<string> SearchOrganisations(
        [Description(
            "Keyword to search for in organisation name or description, " +
            "e.g. 'dementia', 'foodbank', 'carers', 'Age UK', 'NHS'.")]
        string? keyword = null,
        [Description(
            "Restrict results to a specific feed URL. " +
            "If omitted, all configured feeds are searched.")]
        string? feedUrl = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "SearchOrganisations: keyword='{Keyword}', feedUrl='{FeedUrl}'.",
            keyword ?? "(any)", feedUrl ?? "(all feeds)");

        var targets = ResolveFeedTargets(feedUrl);
        if (targets.Count == 0)
            return JsonSerializer.Serialize(new { error = "No ORUK feeds are configured." });

        var maxTotal = options.Value.MaxResultsPerQuery;
        var perFeedLimit = Math.Max(1, (int)Math.Ceiling((double)maxTotal / targets.Count));

        var tasks = targets.Select(async feed =>
        {
            try
            {
                var results = new List<(OrukModels.Models.OrukOrganization Org, Uri Feed)>();
                await foreach (var org in organizationClient.SearchAsync(
                    feed, keyword, perFeedLimit, cancellationToken))
                {
                    results.Add((org, feed));
                }

                logger.LogInformation(
                    "SearchOrganisations: feed {Feed} returned {Count} organisation(s).",
                    feed, results.Count);

                return results;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SearchOrganisations: search failed for feed {Feed}. Skipping.", feed);
                return new List<(OrukModels.Models.OrukOrganization, Uri)>();
            }
        });

        var allResults = await Task.WhenAll(tasks);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = allResults
            .SelectMany(r => r)
            .Where(r => seen.Add(r.Item1.Id))
            .Take(maxTotal)
            .ToList();

        if (unique.Count == 0)
            logger.LogWarning("SearchOrganisations: no organisations found for keyword '{Keyword}'.", keyword);

        var summaries = unique.Select(r =>
        {
            var org = r.Item1;
            var website = org.Url ?? org.Website;
            var phone = org.Phones.Select(p => p.Number).FirstOrDefault();
            var contact = org.Contacts.FirstOrDefault();

            return new
            {
                id = org.Id,
                feed_url = r.Item2.ToString(),
                name = org.Name,
                alternate_name = org.AlternateName,
                description = org.Description is { Length: > 0 }
                    ? (org.Description.Length > 300 ? org.Description[..300].TrimEnd() + "…" : org.Description)
                    : null,
                legal_status = org.LegalStatus,
                year_incorporated = org.YearIncorporated,
                email = org.Email ?? contact?.Email,
                phone,
                website,
                service_count = org.Services.Count > 0 ? org.Services.Count : (int?)null
            };
        }).ToList();

        return JsonSerializer.Serialize(
            new { count = summaries.Count, organisations = summaries }, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Retrieve the full profile of an organisation that delivers Open Referral UK services, " +
        "including its description, contact details, website, legal status, funding, " +
        "and the services it runs. Use this after search_organisations identifies a relevant provider, " +
        "or when the user asks 'Tell me more about Bristol Mind', " +
        "'What services does Carers Support Centre offer?', " +
        "or 'How do I contact the organisation running this service?'.")]
    public async Task<string> GetOrganisationDetail(
        [Description(
            "The ORUK organisation ID (UUID) — obtained from search_organisations or " +
            "the organization.id field in a service record.")]
        string organisationId,
        [Description(
            "The feed URL the organisation belongs to — obtained from search_organisations " +
            "or get_service_detail. Required to route the request to the correct endpoint.")]
        string feedUrl,
        [Description(
            "If true, also fetches the services delivered by this organisation from the " +
            "feed's /services endpoint and includes them in the response. Default is false " +
            "to keep responses concise.")]
        bool includeServices = false,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GetOrganisationDetail: id='{Id}', feed='{Feed}', includeServices={IncludeServices}.",
            organisationId, feedUrl, includeServices);

        if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
        {
            logger.LogError("GetOrganisationDetail: invalid feed URL '{FeedUrl}'.", feedUrl);
            return JsonSerializer.Serialize(new { error = $"Invalid feed URL: {feedUrl}" });
        }

        var org = await organizationClient.GetByIdAsync(feedUri, organisationId, cancellationToken);
        if (org is null)
        {
            logger.LogWarning("GetOrganisationDetail: organisation '{Id}' not found at {Feed}.", organisationId, feedUrl);
            return JsonSerializer.Serialize(new { error = $"Organisation '{organisationId}' not found." });
        }

        logger.LogInformation("GetOrganisationDetail: retrieved organisation '{Name}'.", org.Name);

        // Build contacts list
        var contacts = org.Contacts.Select(c => new
        {
            name = c.Name,
            title = c.Title,
            email = c.Email,
            phone = org.Phones.Select(p => p.Number).FirstOrDefault()
        }).ToList();

        // Build locations list from the organisation's own locations
        var locations = org.Locations.Select(l =>
        {
            var addr = l.PhysicalAddresses.FirstOrDefault();
            return new
            {
                name = l.Name,
                type = l.LocationType,
                address = addr is null
                    ? null
                    : string.Join(", ",
                        new[] { addr.Address1, addr.City, addr.StateProvince, addr.PostalCode }
                        .Where(p => !string.IsNullOrWhiteSpace(p)))
            };
        }).ToList();

        // Embedded services (if the endpoint returned them with the org record)
        List<object>? embeddedServices = null;
        if (org.Services.Count > 0)
        {
            embeddedServices = org.Services.Select(s => (object)new
            {
                id = s.Id,
                name = s.Name,
                status = s.Status,
                description = s.Description is { Length: > 0 }
                    ? (s.Description.Length > 150 ? s.Description[..150].TrimEnd() + "…" : s.Description)
                    : null
            }).ToList();
        }

        // Optionally search for services by this organisation if not embedded
        List<object>? fetchedServices = null;
        if (includeServices && embeddedServices is null)
        {
            try
            {
                var serviceQuery = new OrukServiceQuery { MaxRecords = options.Value.MaxResultsPerQuery };
                var found = new List<object>();
                await foreach (var svc in serviceClient.SearchAsync(feedUri, serviceQuery, cancellationToken))
                {
                    if (svc.Organization?.Id?.Equals(organisationId, StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        found.Add(new
                        {
                            id = svc.Id,
                            name = svc.Name,
                            status = svc.Status,
                            description = svc.Description is { Length: > 0 }
                                ? (svc.Description.Length > 150 ? svc.Description[..150].TrimEnd() + "…" : svc.Description)
                                : null
                        });
                    }
                }
                fetchedServices = found;
                logger.LogInformation(
                    "GetOrganisationDetail: found {Count} service(s) for organisation '{Name}'.",
                    found.Count, org.Name);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "GetOrganisationDetail: failed to fetch services for organisation '{Id}'.",
                    organisationId);
            }
        }

        var website = org.Url ?? org.Website;

        var result = new
        {
            id = org.Id,
            feed_url = feedUrl,
            name = org.Name,
            alternate_name = org.AlternateName,
            description = org.Description,
            legal_status = org.LegalStatus,
            year_incorporated = org.YearIncorporated,
            email = org.Email,
            website,
            uri = org.Uri,
            logo = org.Logo,
            parent_organization_id = org.ParentOrganizationId,
            contacts = contacts.Count > 0 ? contacts : null,
            phones = org.Phones.Select(p => p.Number).Where(n => n is not null).ToList() is { Count: > 0 } phones
                ? phones
                : null,
            locations = locations.Count > 0 ? locations : null,
            funding = org.Funding.Select(f => f.Source).Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                is { Count: > 0 } funding ? funding : null,
            services = embeddedServices ?? fetchedServices
        };

        return JsonSerializer.Serialize(result, JsonOptions);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private List<Uri> ResolveFeedTargets(string? feedUrl)
    {
        if (!string.IsNullOrWhiteSpace(feedUrl))
        {
            if (Uri.TryCreate(feedUrl, UriKind.Absolute, out var uri))
                return [uri];

            logger.LogWarning("SearchOrganisations: invalid feedUrl '{FeedUrl}'. Searching all feeds.", feedUrl);
        }

        if (feedUrls.Count == 0)
            logger.LogError("SearchOrganisations: no ORUK feeds configured.");

        return feedUrls.ToList();
    }
}
