using System.Text.Json;
using OrukModels.Models;
using OrukModels.SchemaOrg;
using OrukTransformer.Core.Mapping;
using OrukTransformer.Core.Vodim;

namespace OrukTransformer.Core.Tests.Mapping;

/// <summary>
/// Tests for <see cref="OrukToSchemaOrgTransformer"/>.
///
/// Tests cover:
/// <list type="bullet">
///   <item>Service scalar field mapping (name, url, email, status, etc.)</item>
///   <item>VODIM classification correctness for valid, other, default, invalid, and missing values</item>
///   <item>Organization mapping</item>
///   <item>Location / Place mapping (geo, address, accessibility)</item>
///   <item>Schedule → OpeningHoursSpecification (RRULE byday expansion)</item>
///   <item>CostOption → Offer (price, currency, defaults)</item>
///   <item>Eligibility → Audience / PeopleAudience</item>
///   <item>ServiceArea → AdministrativeArea</item>
///   <item>Language mapping</item>
///   <item>End-to-end Bristol OPD fixture transformation</item>
/// </list>
/// </summary>
public class OrukToSchemaOrgTransformerTests
{
    private static readonly OrukToSchemaOrgTransformer _sut = new();
    private static readonly TransformationOptions _opts = new()
    {
        BaseUrl = "https://test.example.org",
        DefaultCurrency = "GBP",
    };

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static OrukService MinimalService(string id = "svc-1", string name = "Test Service") =>
        new() { Id = id, Name = name };

    private static FieldMappingRecord? FindRecord(
        TransformationReport report, string sourcePathContains) =>
        report.Records.FirstOrDefault(r =>
            r.SourcePath.Contains(sourcePathContains, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<FieldMappingRecord> FindRecords(
        TransformationReport report, string sourcePathContains) =>
        report.Records.Where(r =>
            r.SourcePath.Contains(sourcePathContains, StringComparison.OrdinalIgnoreCase))
            .ToList();

    // ── Output document structure ─────────────────────────────────────────────────

    [Fact]
    public void Transform_MinimalService_ProducesSchemaOrgDocument()
    {
        var result = _sut.Transform(MinimalService(), _opts);

        Assert.NotNull(result.Document);
        Assert.Equal("https://schema.org", result.Document.Context);
        Assert.NotEmpty(result.Document.Graph);
    }

    [Fact]
    public void Transform_MinimalService_GraphContainsGovernmentService()
    {
        var result = _sut.Transform(MinimalService(), _opts);

        var service = result.Document.Graph.OfType<SchemaOrgGovernmentService>().FirstOrDefault();
        Assert.NotNull(service);
        Assert.Equal("Test Service", service.Name);
    }

    [Fact]
    public void Transform_ServiceWithOrganization_GraphContainsBothNodes()
    {
        var service = MinimalService();
        service.Organization = new OrukOrganization { Id = "org-1", Name = "Test Org" };

        var result = _sut.Transform(service, _opts);

        Assert.Single(result.Document.Graph.OfType<SchemaOrgGovernmentService>());
        Assert.Single(result.Document.Graph.OfType<SchemaOrgOrganization>());
    }

    [Fact]
    public void Transform_ServiceWithLocation_GraphContainsPlace()
    {
        var service = MinimalService();
        service.ServiceAtLocations.Add(new OrukServiceAtLocation
        {
            Id = "sal-1",
            Location = new OrukLocation { Id = "loc-1", Name = "Test Location" }
        });

        var result = _sut.Transform(service, _opts);

        Assert.Single(result.Document.Graph.OfType<SchemaOrgPlace>());
    }

    // ── @id URI construction ──────────────────────────────────────────────────────

    [Fact]
    public void Transform_ServiceId_MappedToSchemaOrgId()
    {
        var result = _sut.Transform(MinimalService("abc-123"), _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Equal("https://test.example.org/services/abc-123", node.Id);
    }

    [Fact]
    public void Transform_OrganizationId_MappedToSchemaOrgId()
    {
        var service = MinimalService();
        service.Organization = new OrukOrganization { Id = "org-99", Name = "Org" };

        var result = _sut.Transform(service, _opts);
        var org = result.Document.Graph.OfType<SchemaOrgOrganization>().Single();

        Assert.Equal("https://test.example.org/organisations/org-99", org.Id);
    }

    [Fact]
    public void Transform_LocationId_MappedToSchemaOrgId()
    {
        var service = MinimalService();
        service.ServiceAtLocations.Add(new OrukServiceAtLocation
        {
            Id = "sal-1",
            Location = new OrukLocation { Id = "loc-42", Name = "Loc" }
        });

        var result = _sut.Transform(service, _opts);
        var place = result.Document.Graph.OfType<SchemaOrgPlace>().Single();

        Assert.Equal("https://test.example.org/locations/loc-42", place.Id);
    }

    // ── VODIM: Valid classifications ──────────────────────────────────────────────

    [Fact]
    public void Transform_ValidServiceName_RecordsValid()
    {
        var result = _sut.Transform(MinimalService(name: "Great Service"), _opts);
        var rec = FindRecord(result.Report, "service.name");

        Assert.Equal(VodimClassification.Valid, rec?.Classification);
    }

    [Fact]
    public void Transform_ValidUrl_RecordsValid()
    {
        var service = MinimalService();
        service.Url = "https://example.org/service";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.url");

        Assert.Equal(VodimClassification.Valid, rec?.Classification);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();
        Assert.Equal("https://example.org/service", node.Url);
    }

    [Fact]
    public void Transform_ValidEmail_RecordsValid()
    {
        var service = MinimalService();
        service.Email = "info@example.org";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.email");

        Assert.Equal(VodimClassification.Valid, rec?.Classification);
    }

    [Fact]
    public void Transform_ValidOrukStatus_RecordsValid()
    {
        var service = MinimalService();
        service.Status = "active";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.status");

        Assert.Equal(VodimClassification.Valid, rec?.Classification);
    }

    [Fact]
    public void Transform_ValidAssuredDate_RecordsValid()
    {
        var service = MinimalService();
        service.AssuredDate = "2025-09-02";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.assured_date");

        Assert.Equal(VodimClassification.Valid, rec?.Classification);
    }

    // ── VODIM: Missing classifications ────────────────────────────────────────────

    [Fact]
    public void Transform_NullServiceName_RecordsMissing()
    {
        var service = new OrukService { Id = "s1", Name = "" };
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.name");

        Assert.Equal(VodimClassification.Missing, rec?.Classification);
    }

    [Fact]
    public void Transform_NullUrl_RecordsMissing()
    {
        var result = _sut.Transform(MinimalService(), _opts);
        var rec = FindRecord(result.Report, "service.url");

        Assert.Equal(VodimClassification.Missing, rec?.Classification);
    }

    [Fact]
    public void Transform_NullEmail_RecordsMissing()
    {
        var result = _sut.Transform(MinimalService(), _opts);
        var rec = FindRecord(result.Report, "service.email");

        Assert.Equal(VodimClassification.Missing, rec?.Classification);
    }

    [Fact]
    public void Transform_NullOrganization_RecordsMissing()
    {
        var result = _sut.Transform(MinimalService(), _opts);
        var rec = FindRecord(result.Report, "service.organization");

        Assert.Equal(VodimClassification.Missing, rec?.Classification);
    }

    // ── VODIM: Invalid classifications ────────────────────────────────────────────

    [Fact]
    public void Transform_InvalidUrl_RecordsInvalid_AndOmitsFromOutput()
    {
        var service = MinimalService();
        service.Url = "not-a-url";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.url");

        Assert.Equal(VodimClassification.Invalid, rec?.Classification);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();
        Assert.Null(node.Url);
    }

    [Fact]
    public void Transform_InvalidEmail_RecordsInvalid()
    {
        var service = MinimalService();
        service.Email = "not-an-email";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.email");

        Assert.Equal(VodimClassification.Invalid, rec?.Classification);
    }

    [Fact]
    public void Transform_InvalidDate_RecordsInvalid()
    {
        var service = MinimalService();
        service.AssuredDate = "not-a-date";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.assured_date");

        Assert.Equal(VodimClassification.Invalid, rec?.Classification);
    }

    // ── VODIM: Other classifications ──────────────────────────────────────────────

    [Fact]
    public void Transform_UnrecognisedOrukStatus_RecordsOther()
    {
        var service = MinimalService();
        service.Status = "pending-review";
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.status");

        Assert.Equal(VodimClassification.Other, rec?.Classification);
    }

    [Fact]
    public void Transform_SentinelMinAge_RecordsOther()
    {
        // Bristol OPD uses -1 as "no minimum age" — non-standard but recognisable
        var service = MinimalService();
        service.MinimumAge = -1;
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.minimum_age");

        Assert.Equal(VodimClassification.Other, rec?.Classification);
    }

    [Fact]
    public void Transform_SentinelMaxAge_RecordsOther()
    {
        var service = MinimalService();
        service.MaximumAge = -1;
        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "service.maximum_age");

        Assert.Equal(VodimClassification.Other, rec?.Classification);
    }

    // ── VODIM: Default classifications ────────────────────────────────────────────

    [Fact]
    public void Transform_CostOptionWithNoCurrency_RecordsDefault()
    {
        var service = MinimalService();
        service.CostOptions.Add(new OrukCostOption { Id = "co-1", Amount = 5m });
        var result = _sut.Transform(service, _opts);
        var rec = FindRecords(result.Report, "currency").FirstOrDefault();

        Assert.Equal(VodimClassification.Default, rec?.Classification);
        Assert.Equal("GBP", rec?.MappedValue);
    }

    [Fact]
    public void Transform_CostOptionWithNoCurrency_UsesDefaultCurrencyInOutput()
    {
        var service = MinimalService();
        service.CostOptions.Add(new OrukCostOption { Id = "co-1", Amount = 10m });
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Single(node.Offers!);
        Assert.Equal("GBP", node.Offers![0].PriceCurrency);
    }

    // ── Schedule mapping ──────────────────────────────────────────────────────────

    [Fact]
    public void Transform_ScheduleWithRruleByDay_ExpandsIntoDayPerEntry()
    {
        var service = MinimalService();
        service.Schedules.Add(new OrukSchedule
        {
            Id = "sch-1",
            ByDay = "MO,WE,FR",
            OpensAt = "09:00",
            ClosesAt = "17:00",
        });

        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Equal(3, node.OpeningHoursSpecification!.Count);
        var days = node.OpeningHoursSpecification.Select(o => o.DayOfWeek).ToHashSet();
        Assert.Contains(SchemaDayOfWeek.Monday, days);
        Assert.Contains(SchemaDayOfWeek.Wednesday, days);
        Assert.Contains(SchemaDayOfWeek.Friday, days);
    }

    [Fact]
    public void Transform_ScheduleWithValidByDay_RecordsValidForEachDay()
    {
        var service = MinimalService();
        service.Schedules.Add(new OrukSchedule
        {
            Id = "sch-1",
            ByDay = "MO,TU",
            OpensAt = "09:00",
            ClosesAt = "17:00",
        });

        var result = _sut.Transform(service, _opts);
        var recs = FindRecords(result.Report, "byday");

        Assert.All(recs, r => Assert.Equal(VodimClassification.Valid, r.Classification));
    }

    [Fact]
    public void Transform_ScheduleWithLongFormDay_RecordsOther()
    {
        var service = MinimalService();
        service.Schedules.Add(new OrukSchedule
        {
            Id = "sch-1",
            ByDay = "Monday",
            OpensAt = "09:00",
        });

        var result = _sut.Transform(service, _opts);
        var rec = FindRecords(result.Report, "byday").FirstOrDefault();

        Assert.Equal(VodimClassification.Other, rec?.Classification);
    }

    [Fact]
    public void Transform_ScheduleWithOrdinalByDay_StripsOrdinalAndRecordsValid()
    {
        // "2MO" = second Monday of the month; ordinal info is lost but day maps
        var service = MinimalService();
        service.Schedules.Add(new OrukSchedule
        {
            Id = "sch-1",
            ByDay = "2MO",
            OpensAt = "10:00",
        });

        var result = _sut.Transform(service, _opts);
        var rec = FindRecords(result.Report, "byday").FirstOrDefault();

        // Valid because the day code resolved; note should mention ordinal strip
        Assert.Equal(VodimClassification.Valid, rec?.Classification);
        Assert.Contains("Ordinal", rec?.Note ?? string.Empty);
    }

    [Fact]
    public void Transform_ScheduleWithUnrecognisedDay_RecordsInvalid()
    {
        var service = MinimalService();
        service.Schedules.Add(new OrukSchedule { Id = "sch-1", ByDay = "FOOBAR" });

        var result = _sut.Transform(service, _opts);
        var rec = FindRecords(result.Report, "byday").FirstOrDefault();

        Assert.Equal(VodimClassification.Invalid, rec?.Classification);
    }

    [Fact]
    public void Transform_ScheduleWithValidTimes_RecordsValid()
    {
        var service = MinimalService();
        service.Schedules.Add(new OrukSchedule
        {
            Id = "sch-1",
            ByDay = "MO",
            OpensAt = "09:00",
            ClosesAt = "17:30",
        });

        var result = _sut.Transform(service, _opts);
        var opensRec = FindRecord(result.Report, "opens_at");
        var closesRec = FindRecord(result.Report, "closes_at");

        Assert.Equal(VodimClassification.Valid, opensRec?.Classification);
        Assert.Equal(VodimClassification.Valid, closesRec?.Classification);
    }

    // ── CostOption → Offer ────────────────────────────────────────────────────────

    [Fact]
    public void Transform_FreeCostOption_SetsPrice0()
    {
        var service = MinimalService();
        service.CostOptions.Add(new OrukCostOption { Id = "co-1", Option = "free" });
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Equal(0m, node.Offers![0].Price);
    }

    [Fact]
    public void Transform_NumericalCostOption_MapsPrice()
    {
        var service = MinimalService();
        service.CostOptions.Add(new OrukCostOption
        {
            Id = "co-1",
            Amount = 12.50m,
            Currency = "GBP",
        });
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Equal(12.50m, node.Offers![0].Price);
        Assert.Equal("GBP", node.Offers[0].PriceCurrency);
    }

    [Fact]
    public void Transform_NegativeCostOption_RecordsInvalidAndOmitsOffer()
    {
        var service = MinimalService();
        service.CostOptions.Add(new OrukCostOption { Id = "co-1", Amount = -5m });
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Null(node.Offers);
        var rec = FindRecord(result.Report, "cost_options[0].amount");
        Assert.Equal(VodimClassification.Invalid, rec?.Classification);
    }

    // ── Eligibility → Audience ────────────────────────────────────────────────────

    [Fact]
    public void Transform_AgeConstraints_ProducesPeopleAudience()
    {
        var service = MinimalService();
        service.MinimumAge = 16;
        service.MaximumAge = 25;
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.IsType<SchemaOrgPeopleAudience>(node.Audience);
        var pa = (SchemaOrgPeopleAudience)node.Audience!;
        Assert.Equal(16, pa.SuggestedMinAge);
        Assert.Equal(25, pa.SuggestedMaxAge);
    }

    [Fact]
    public void Transform_EligibilityDescription_ProducesAudience()
    {
        var service = MinimalService();
        service.EligibilityDescription = "Open to all Bristol residents";
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.NotNull(node.Audience);
        Assert.Equal("Open to all Bristol residents", node.Audience!.Description);
    }

    [Fact]
    public void Transform_SentinelAges_ProducesNoAudienceAgeConstraint()
    {
        // Bristol OPD uses -1; audience should still be created from eligibility_description
        // but no age constraint (Other classification, value omitted from audience)
        var service = MinimalService();
        service.MinimumAge = -1;
        service.MaximumAge = -1;
        service.EligibilityDescription = "Anyone";
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        // Audience from description, but NOT PeopleAudience (no valid age range)
        Assert.IsNotType<SchemaOrgPeopleAudience>(node.Audience);
        Assert.Equal("Anyone", node.Audience!.Description);
    }

    // ── ServiceArea → AdministrativeArea ─────────────────────────────────────────

    [Fact]
    public void Transform_ServiceAreas_ProduceAdministrativeAreas()
    {
        var service = MinimalService();
        service.ServiceAreas.Add(new OrukServiceArea { Id = "sa-1", Name = "Bristol" });
        service.ServiceAreas.Add(new OrukServiceArea { Id = "sa-2", Name = "South Gloucestershire" });

        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Equal(2, node.AreaServed!.Count);
    }

    // ── Language mapping ──────────────────────────────────────────────────────────

    [Fact]
    public void Transform_Languages_MappedToAvailableLanguage()
    {
        var service = MinimalService();
        service.Languages.Add(new OrukLanguage { Id = "lang-1", Name = "Welsh", Code = "cy" });
        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Single(node.AvailableLanguage!);
        Assert.Equal("Welsh", node.AvailableLanguage![0].Name);
        Assert.Equal("cy", node.AvailableLanguage[0].Identifier);
    }

    // ── GeoCoordinates ────────────────────────────────────────────────────────────

    [Fact]
    public void Transform_ValidLatLng_ProducesGeoCoordinates()
    {
        var service = MinimalService();
        service.ServiceAtLocations.Add(new OrukServiceAtLocation
        {
            Id = "sal-1",
            Location = new OrukLocation
            {
                Id = "loc-1",
                Latitude = 51.4545,
                Longitude = -2.5879,
            }
        });

        var result = _sut.Transform(service, _opts);
        var place = result.Document.Graph.OfType<SchemaOrgPlace>().Single();

        Assert.NotNull(place.Geo);
        Assert.Equal((decimal)51.4545, place.Geo!.Latitude);
        Assert.Equal((decimal)-2.5879, place.Geo.Longitude);
    }

    [Fact]
    public void Transform_InvalidLatLng_RecordsInvalid()
    {
        var service = MinimalService();
        service.ServiceAtLocations.Add(new OrukServiceAtLocation
        {
            Id = "sal-1",
            Location = new OrukLocation
            {
                Id = "loc-1",
                Latitude = 999.0,  // out of range
                Longitude = 0.0,
            }
        });

        var result = _sut.Transform(service, _opts);
        var place = result.Document.Graph.OfType<SchemaOrgPlace>().Single();
        var rec = FindRecord(result.Report, "latitude");

        Assert.Equal(VodimClassification.Invalid, rec?.Classification);
        Assert.Null(place.Geo);
    }

    // ── Organization mapping ──────────────────────────────────────────────────────

    [Fact]
    public void Transform_OrganizationLogo_MapsToImageObject()
    {
        var service = MinimalService();
        service.Organization = new OrukOrganization
        {
            Id = "org-1",
            Name = "Test Org",
            Logo = "https://example.org/logo.png",
        };

        var result = _sut.Transform(service, _opts);
        var org = result.Document.Graph.OfType<SchemaOrgOrganization>().Single();

        Assert.NotNull(org.Logo);
        Assert.Equal("https://example.org/logo.png", org.Logo!.Url);
    }

    [Fact]
    public void Transform_OrganizationUri_MapsToSameAs()
    {
        var service = MinimalService();
        service.Organization = new OrukOrganization
        {
            Id = "org-1",
            Name = "Test Org",
            Uri = "https://data.bristol.gov.uk/org/42",
        };

        var result = _sut.Transform(service, _opts);
        var org = result.Document.Graph.OfType<SchemaOrgOrganization>().Single();

        Assert.Equal("https://data.bristol.gov.uk/org/42", org.SameAs);
    }

    [Fact]
    public void Transform_OrganizationWebsiteFallback_RecordsOther()
    {
        var service = MinimalService();
        service.Organization = new OrukOrganization
        {
            Id = "org-1",
            Name = "Test Org",
            Website = "http://example.org",  // non-standard field
        };

        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "organization.website");

        Assert.Equal(VodimClassification.Other, rec?.Classification);
    }

    // ── Report summary ────────────────────────────────────────────────────────────

    [Fact]
    public void Transform_Report_ContainsTotalCountGreaterThanZero()
    {
        var result = _sut.Transform(MinimalService(), _opts);

        Assert.True(result.Report.TotalCount > 0);
    }

    [Fact]
    public void Transform_Report_SummaryStringContainsServiceId()
    {
        var result = _sut.Transform(MinimalService("svc-abc"), _opts);

        Assert.Contains("svc-abc", result.Report.Summary());
    }

    [Fact]
    public void Transform_Report_ValidCountGreaterThanZeroForMinimalService()
    {
        // Even a minimal service should have at least its ID and name recorded as Valid
        var result = _sut.Transform(MinimalService(), _opts);

        Assert.True(result.Report.ValidCount > 0);
    }

    // ── Bristol OPD end-to-end fixture ────────────────────────────────────────────

    [Fact]
    public void Transform_BristolFixture_ProducesDocumentWithoutException()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));

        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(service);
        var result = _sut.Transform(service!, _opts);

        Assert.NotNull(result.Document);
        Assert.NotEmpty(result.Document.Graph);
    }

    [Fact]
    public void Transform_BristolFixture_ServiceNodeHasExpectedName()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.Equal("1625 Independent People", node.Name);
    }

    [Fact]
    public void Transform_BristolFixture_OrganizationNodePresent()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);

        Assert.Single(result.Document.Graph.OfType<SchemaOrgOrganization>());
    }

    [Fact]
    public void Transform_BristolFixture_ServiceAreasPresent()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.NotNull(node.AreaServed);
        Assert.NotEmpty(node.AreaServed!);
    }

    [Fact]
    public void Transform_BristolFixture_SentinelAgesRecordedAsOther()
    {
        // Bristol OPD sends minimum_age: -1 and maximum_age: -1
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);
        var minRec = FindRecord(result.Report, "minimum_age");
        var maxRec = FindRecord(result.Report, "maximum_age");

        Assert.Equal(VodimClassification.Other, minRec?.Classification);
        Assert.Equal(VodimClassification.Other, maxRec?.Classification);
    }

    [Fact]
    public void Transform_BristolFixture_AssuredDateRecordedAsValid()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);
        var rec = FindRecord(result.Report, "assured_date");

        Assert.Equal(VodimClassification.Valid, rec?.Classification);
    }

    [Fact]
    public void Transform_BristolFixture_ReportHasNoInvalidClassifications()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);
        var invalids = result.Report.ByClassification(VodimClassification.Invalid);

        // Bristol fixture should have no invalid fields — log them if found for visibility
        Assert.True(invalids.Count == 0,
            $"Expected 0 Invalid records but got {invalids.Count}: " +
            string.Join("; ", invalids.Select(r => r.SourcePath)));
    }

    [Fact]
    public void Transform_BristolFixture_AdditionalPropertyIncludesOrukStatus()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "bristol-service-1625ip.json"));
        var service = JsonSerializer.Deserialize<OrukService>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var result = _sut.Transform(service, _opts);
        var node = result.Document.Graph.OfType<SchemaOrgGovernmentService>().Single();

        Assert.NotNull(node.AdditionalProperty);
        var statusProp = node.AdditionalProperty!.FirstOrDefault(p => p.Name == "orukStatus");
        Assert.NotNull(statusProp);
        Assert.Equal("active", statusProp!.Value.ToString());
    }
}
