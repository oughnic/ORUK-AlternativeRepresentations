using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OrukApiClient;
using OrukTransformer.Mcp;
using OrukTransformer.Mcp.Config;
using OrukModels.Models;

namespace OrukTransformer.Mcp.Tools;

/// <summary>
/// MCP tool for querying the opening hours and availability schedule of a specific
/// ORUK service. Avoids the AI agent needing to parse a full service record just
/// to answer "when is this open?".
/// </summary>
[McpServerToolType]
public sealed class OrukScheduleTool(
    IOrukServiceClient serviceClient,
    IFeedRegistry feedRegistry,
    ILogger<OrukScheduleTool> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [McpServerTool]
    [Description(
        "Get the opening hours and availability schedule for a specific Open Referral UK service. " +
        "Use this when the user asks when a service is open, whether it runs at weekends, " +
        "or whether there are evening sessions. Use the feed_url and service_id values " +
        "returned by search_services.")]
    public async Task<string> GetServiceSchedule(
        [Description("The base URL or configured feed name (from list_feeds) where the service was found.")]
        string feedUrl,
        [Description("The unique service ID (returned by search_services).")]
        string serviceId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "GetServiceSchedule: service={ServiceId}, feed={FeedUrl}.", serviceId, feedUrl);

        var feedUri = ResolveFeedUri(feedUrl);
        if (feedUri is null)
        {
            logger.LogWarning("GetServiceSchedule: invalid feed_url '{FeedUrl}'.", feedUrl);
            return JsonSerializer.Serialize(new
            {
                error = "Invalid feed_url value. Provide a configured feed name or absolute URL."
            });
        }

        var service = await serviceClient.GetByIdAsync(feedUri, serviceId, cancellationToken);

        if (service is null)
        {
            logger.LogWarning(
                "GetServiceSchedule: service {ServiceId} not found in feed {FeedUrl}.",
                serviceId, feedUrl);
            return JsonSerializer.Serialize(
                new { error = $"Service '{serviceId}' was not found in feed '{feedUrl}'." });
        }

        // Collect schedules from the service itself and from each service-at-location
        var scheduleGroups = new List<object>();

        var serviceSchedules = FormatSchedules(service.Schedules);
        if (serviceSchedules.Count > 0)
        {
            scheduleGroups.Add(new
            {
                context = "Service (general)",
                schedules = serviceSchedules
            });
        }

        foreach (var sal in service.ServiceAtLocations)
        {
            var locationName = sal.Location?.Name
                ?? sal.Location?.PhysicalAddresses.FirstOrDefault()?.City
                ?? "Unknown location";

            var locationSchedules = FormatSchedules(sal.Schedules);
            if (locationSchedules.Count > 0)
            {
                scheduleGroups.Add(new
                {
                    context = locationName,
                    schedules = locationSchedules
                });
            }
        }

        var hasScheduleData = scheduleGroups.Count > 0;

        logger.LogInformation(
            "GetServiceSchedule: '{ServiceName}' — {Groups} schedule group(s) found.",
            service.Name, scheduleGroups.Count);

        if (!hasScheduleData)
            logger.LogWarning(
                "GetServiceSchedule: no schedule data available for service {ServiceId}.", serviceId);

        return JsonSerializer.Serialize(new
        {
            service_id = serviceId,
            service_name = service.Name,
            feed_url = feedUri.ToString(),
            feed_name = feedRegistry.GetDisplayName(feedUri),
            has_schedule_data = hasScheduleData,
            schedule_groups = scheduleGroups.Count > 0 ? scheduleGroups : null,
            alert = PlainTextSanitizer.ToPlainText(service.Alert)
        }, JsonOptions);
    }

    private Uri? ResolveFeedUri(string feedIdentifier)
    {
        var configured = feedRegistry.Resolve(feedIdentifier);
        if (configured is not null)
            return configured.Url;

        if (Uri.TryCreate(feedIdentifier, UriKind.Absolute, out var supplied))
            return supplied;

        return null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static List<object> FormatSchedules(IEnumerable<OrukSchedule> schedules)
    {
        var result = new List<object>();

        foreach (var s in schedules)
        {
            // Skip completely empty schedule records
            if (s.Description is null && s.OpensAt is null && s.ByDay is null &&
                s.Freq is null && s.ValidFrom is null)
                continue;

            result.Add(new
            {
                description = PlainTextSanitizer.ToPlainText(s.Description),
                days = FormatDays(s.ByDay),
                opens_at = s.OpensAt,
                closes_at = s.ClosesAt,
                recurrence = FormatRecurrence(s),
                valid_from = s.ValidFrom,
                valid_to = s.ValidTo,
                attending_type = PlainTextSanitizer.ToPlainText(s.AttendingType),
                notes = PlainTextSanitizer.ToPlainText(s.Notes),
                schedule_link = s.ScheduleLink
            });
        }

        return result;
    }

    /// <summary>
    /// Expands iCal BYDAY shortcodes into readable day names.
    /// e.g. "MO,WE,FR" → "Monday, Wednesday, Friday"
    /// </summary>
    private static string? FormatDays(string? byDay)
    {
        if (string.IsNullOrWhiteSpace(byDay)) return null;

        var dayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["MO"] = "Monday", ["TU"] = "Tuesday", ["WE"] = "Wednesday",
            ["TH"] = "Thursday", ["FR"] = "Friday", ["SA"] = "Saturday", ["SU"] = "Sunday"
        };

        var parts = byDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var names = parts.Select(p => dayMap.TryGetValue(p.ToUpperInvariant(), out var name) ? name : p);
        return string.Join(", ", names);
    }

    /// <summary>
    /// Formats an iCal FREQ / INTERVAL / UNTIL pattern into a readable string.
    /// e.g. WEEKLY / every 2 weeks until 2025-12-31
    /// </summary>
    private static string? FormatRecurrence(OrukSchedule s)
    {
        if (s.Freq is null) return null;

        var freq = s.Freq.ToUpperInvariant() switch
        {
            "DAILY" => "Daily",
            "WEEKLY" => "Weekly",
            "MONTHLY" => "Monthly",
            "YEARLY" => "Yearly",
            _ => s.Freq
        };

        var parts = new List<string> { freq };

        if (s.Interval is > 1)
            parts.Add($"every {s.Interval} {freq.ToLowerInvariant()}s");

        if (!string.IsNullOrWhiteSpace(s.Until))
            parts.Add($"until {s.Until}");
        else if (s.Count.HasValue)
            parts.Add($"for {s.Count} occurrence(s)");

        return string.Join(", ", parts);
    }
}
