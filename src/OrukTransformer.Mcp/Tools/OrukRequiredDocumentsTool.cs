using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OrukApiClient;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool that answers the practical "what do I need?" question for a specific
/// ORUK service — returning required documents, application process notes, and
/// eligibility conditions in one focused response.
/// </summary>
[McpServerToolType]
public sealed class OrukRequiredDocumentsTool(
    IOrukServiceClient serviceClient,
    ILogger<OrukRequiredDocumentsTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Get what a person needs to access a specific Open Referral UK service — including required " +
        "documents, the application process, and eligibility conditions. Use this when the user " +
        "asks 'What do I need to bring?', 'How do I apply?', or 'Am I eligible?'. " +
        "Use the feed_url and service_id values returned by search_services.")]
    public async Task<string> GetRequiredDocuments(
        [Description("The base URL of the ORUK feed the service was found in (returned by search_services).")]
        string feedUrl,
        [Description("The unique service ID (returned by search_services).")]
        string serviceId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GetRequiredDocuments: service={ServiceId}, feed={FeedUrl}.", serviceId, feedUrl);

        if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
        {
            logger.LogWarning("GetRequiredDocuments: invalid feed_url '{FeedUrl}'.", feedUrl);
            return JsonSerializer.Serialize(new { error = "Invalid feed_url value." });
        }

        var service = await serviceClient.GetByIdAsync(feedUri, serviceId, cancellationToken);

        if (service is null)
        {
            logger.LogWarning(
                "GetRequiredDocuments: service {ServiceId} not found in feed {FeedUrl}.",
                serviceId, feedUrl);
            return JsonSerializer.Serialize(
                new { error = $"Service '{serviceId}' was not found in feed '{feedUrl}'." });
        }

        // Required documents
        var documents = service.RequiredDocuments
            .Where(d => !string.IsNullOrWhiteSpace(d.Document))
            .Select(d => new { document = d.Document, uri = d.Uri })
            .ToList();

        // Eligibility conditions (structured + free-text)
        var eligibilityConditions = service.Eligibility
            .Where(e => !string.IsNullOrWhiteSpace(e.Description))
            .Select(e => new
            {
                description = e.Description,
                minimum_age = e.MinimumAge,
                maximum_age = e.MaximumAge
            })
            .ToList<object>();

        // Age range from the service itself (HSDS v3 top-level fields)
        string? ageRange = null;
        if (service.MinimumAge.HasValue || service.MaximumAge.HasValue)
        {
            var min = service.MinimumAge?.ToString() ?? "any";
            var max = service.MaximumAge?.ToString() ?? "any";
            ageRange = $"{min}–{max} years";
        }

        var hasRequirementsData = documents.Count > 0
            || eligibilityConditions.Count > 0
            || !string.IsNullOrWhiteSpace(service.ApplicationProcess)
            || !string.IsNullOrWhiteSpace(service.EligibilityDescription)
            || ageRange is not null;

        logger.LogInformation(
            "GetRequiredDocuments: '{ServiceName}' — {DocCount} document(s), " +
            "application_process={HasProcess}, eligibility={HasEligibility}.",
            service.Name,
            documents.Count,
            !string.IsNullOrWhiteSpace(service.ApplicationProcess),
            eligibilityConditions.Count > 0 || !string.IsNullOrWhiteSpace(service.EligibilityDescription));

        if (!hasRequirementsData)
            logger.LogWarning(
                "GetRequiredDocuments: no requirements or eligibility data found for service {ServiceId}.",
                serviceId);

        return JsonSerializer.Serialize(new
        {
            service_id = serviceId,
            service_name = service.Name,
            feed_url = feedUrl,
            has_requirements_data = hasRequirementsData,
            required_documents = documents.Count > 0 ? documents : null,
            application_process = string.IsNullOrWhiteSpace(service.ApplicationProcess)
                ? null
                : service.ApplicationProcess,
            eligibility_description = string.IsNullOrWhiteSpace(service.EligibilityDescription)
                ? null
                : service.EligibilityDescription,
            eligibility_conditions = eligibilityConditions.Count > 0 ? eligibilityConditions : null,
            age_range = ageRange,
            wait_time = string.IsNullOrWhiteSpace(service.WaitTime) ? null : service.WaitTime
        }, JsonOptions);
    }
}
