using System.Text.Json;
using OrukModels.SchemaOrg;

namespace OrukModels.Tests.SchemaOrg;

/// <summary>
/// Tests for Schema.org model serialisation and required-property enforcement.
/// These tests verify that the types produce correct JSON-LD output and that
/// mandatory Schema.org properties are present and correctly typed.
/// No live HTTP calls are made.
/// </summary>
public class SchemaOrgSerializationTests
{
    // ── SchemaDayOfWeek ───────────────────────────────────────────────────────────

    [Fact]
    public void SchemaDayOfWeek_Monday_HasCorrectUri()
    {
        Assert.Equal("https://schema.org/Monday", SchemaDayOfWeek.Monday);
    }

    [Fact]
    public void SchemaDayOfWeek_Sunday_HasCorrectUri()
    {
        Assert.Equal("https://schema.org/Sunday", SchemaDayOfWeek.Sunday);
    }

    [Fact]
    public void SchemaDayOfWeek_PublicHolidays_HasCorrectUri()
    {
        Assert.Equal("https://schema.org/PublicHolidays", SchemaDayOfWeek.PublicHolidays);
    }

    [Fact]
    public void SchemaDayOfWeek_AllValues_ContainsEightEntries()
    {
        Assert.Equal(8, SchemaDayOfWeek.AllValues.Count);
    }

    [Fact]
    public void SchemaDayOfWeek_AllValues_ContainsAllDays()
    {
        Assert.Contains(SchemaDayOfWeek.Monday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.Tuesday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.Wednesday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.Thursday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.Friday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.Saturday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.Sunday, SchemaDayOfWeek.AllValues);
        Assert.Contains(SchemaDayOfWeek.PublicHolidays, SchemaDayOfWeek.AllValues);
    }

    // ── SchemaOrgDocument ─────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgDocument_Serializes_WithContext()
    {
        var doc = new SchemaOrgDocument();
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("https://schema.org", root.RootElement.GetProperty("@context").GetString());
    }

    [Fact]
    public void SchemaOrgDocument_Serializes_WithEmptyGraph()
    {
        var doc = new SchemaOrgDocument();
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var graph = root.RootElement.GetProperty("@graph");
        Assert.Equal(JsonValueKind.Array, graph.ValueKind);
        Assert.Equal(0, graph.GetArrayLength());
    }

    // ── @type discriminator ───────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgGovernmentService_Serializes_WithCorrectType()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgGovernmentService { Name = "Test Service" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("GovernmentService", node.GetProperty("@type").GetString());
    }

    [Fact]
    public void SchemaOrgService_Serializes_WithCorrectType()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgService { Name = "Test Service" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("Service", node.GetProperty("@type").GetString());
    }

    [Fact]
    public void SchemaOrgOrganization_Serializes_WithCorrectType()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgOrganization { Name = "Test Org" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("Organization", node.GetProperty("@type").GetString());
    }

    [Fact]
    public void SchemaOrgLocalBusiness_Serializes_WithCorrectType()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgLocalBusiness { Name = "Test Business" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("LocalBusiness", node.GetProperty("@type").GetString());
    }

    [Fact]
    public void SchemaOrgPlace_Serializes_WithCorrectType()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgPlace { Name = "Test Place" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("Place", node.GetProperty("@type").GetString());
    }

    [Fact]
    public void SchemaOrgAdministrativeArea_Serializes_WithCorrectType()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgAdministrativeArea { Name = "Bristol" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("AdministrativeArea", node.GetProperty("@type").GetString());
    }

    // ── Null properties omitted ───────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgGovernmentService_OmitsNullProperties()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgGovernmentService { Name = "Test" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.False(node.TryGetProperty("description", out _), "Null description should be omitted");
        Assert.False(node.TryGetProperty("@id", out _), "Null @id should be omitted");
        Assert.False(node.TryGetProperty("url", out _), "Null url should be omitted");
    }

    [Fact]
    public void SchemaOrgGovernmentService_IncludesName()
    {
        var doc = new SchemaOrgDocument
        {
            Graph = [new SchemaOrgGovernmentService { Name = "My Service" }]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var node = root.RootElement.GetProperty("@graph")[0];
        Assert.Equal("My Service", node.GetProperty("name").GetString());
    }

    // ── Mixed-type @graph ─────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgDocument_Graph_CanContainMixedTypes()
    {
        var doc = new SchemaOrgDocument
        {
            Graph =
            [
                new SchemaOrgGovernmentService { Name = "Service A", Id = "https://example.gov.uk/services/a" },
                new SchemaOrgOrganization { Name = "Org A", Id = "https://example.gov.uk/orgs/a" },
                new SchemaOrgPlace { Name = "Location A", Id = "https://example.gov.uk/locations/a" }
            ]
        };
        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        var graph = root.RootElement.GetProperty("@graph");
        Assert.Equal(3, graph.GetArrayLength());
        Assert.Equal("GovernmentService", graph[0].GetProperty("@type").GetString());
        Assert.Equal("Organization", graph[1].GetProperty("@type").GetString());
        Assert.Equal("Place", graph[2].GetProperty("@type").GetString());
    }

    // ── GeoCoordinates ────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgGeoCoordinates_Serializes_WithTypeAndCoordinates()
    {
        var geo = new SchemaOrgGeoCoordinates { Latitude = 51.4545m, Longitude = -2.5879m };
        var json = JsonSerializer.Serialize(geo, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("GeoCoordinates", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal(51.4545m, root.RootElement.GetProperty("latitude").GetDecimal());
        Assert.Equal(-2.5879m, root.RootElement.GetProperty("longitude").GetDecimal());
    }

    // ── Offer ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgOffer_Serializes_WithRequiredProperties()
    {
        var offer = new SchemaOrgOffer { Price = 0m, PriceCurrency = "GBP" };
        var json = JsonSerializer.Serialize(offer, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("Offer", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal(0m, root.RootElement.GetProperty("price").GetDecimal());
        Assert.Equal("GBP", root.RootElement.GetProperty("priceCurrency").GetString());
    }

    [Fact]
    public void SchemaOrgOffer_OmitsNullDescription()
    {
        var offer = new SchemaOrgOffer { Price = 5m, PriceCurrency = "GBP" };
        var json = JsonSerializer.Serialize(offer, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.False(root.RootElement.TryGetProperty("description", out _));
    }

    // ── OpeningHoursSpecification ─────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgOpeningHoursSpecification_Serializes_WithDayOfWeek()
    {
        var spec = new SchemaOrgOpeningHoursSpecification
        {
            DayOfWeek = SchemaDayOfWeek.Monday,
            Opens = "09:00",
            Closes = "17:00"
        };
        var json = JsonSerializer.Serialize(spec, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("OpeningHoursSpecification", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal("https://schema.org/Monday", root.RootElement.GetProperty("dayOfWeek").GetString());
        Assert.Equal("09:00", root.RootElement.GetProperty("opens").GetString());
        Assert.Equal("17:00", root.RootElement.GetProperty("closes").GetString());
    }

    // ── PostalAddress ─────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgPostalAddress_Serializes_WithType()
    {
        var address = new SchemaOrgPostalAddress
        {
            StreetAddress = "1 Test Street",
            AddressLocality = "Bristol",
            PostalCode = "BS1 1AA",
            AddressCountry = "GB"
        };
        var json = JsonSerializer.Serialize(address, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("PostalAddress", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal("1 Test Street", root.RootElement.GetProperty("streetAddress").GetString());
        Assert.Equal("Bristol", root.RootElement.GetProperty("addressLocality").GetString());
        Assert.Equal("BS1 1AA", root.RootElement.GetProperty("postalCode").GetString());
        Assert.Equal("GB", root.RootElement.GetProperty("addressCountry").GetString());
    }

    // ── ContactPoint ─────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgContactPoint_Serializes_WithType()
    {
        var contact = new SchemaOrgContactPoint
        {
            ContactType = "enquiries",
            Telephone = "+44 117 123 4567"
        };
        var json = JsonSerializer.Serialize(contact, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("ContactPoint", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal("enquiries", root.RootElement.GetProperty("contactType").GetString());
    }

    // ── PeopleAudience ────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgPeopleAudience_Serializes_WithCorrectType()
    {
        var audience = new SchemaOrgPeopleAudience { SuggestedMinAge = 16, SuggestedMaxAge = 25 };
        var json = JsonSerializer.Serialize(audience, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("PeopleAudience", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal(16, root.RootElement.GetProperty("suggestedMinAge").GetDouble());
        Assert.Equal(25, root.RootElement.GetProperty("suggestedMaxAge").GetDouble());
    }

    // ── PropertyValue ─────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgPropertyValue_Serializes_WithRequiredProperties()
    {
        var pv = new SchemaOrgPropertyValue { Name = "uprn", Value = "123456789012" };
        var json = JsonSerializer.Serialize(pv, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("PropertyValue", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal("uprn", root.RootElement.GetProperty("name").GetString());
        Assert.Equal("123456789012", root.RootElement.GetProperty("value").GetString());
    }

    [Fact]
    public void SchemaOrgLocationFeatureSpecification_Serializes_WithCorrectType()
    {
        var feature = new SchemaOrgLocationFeatureSpecification
        {
            Name = "Wheelchair access",
            Value = true
        };
        var json = JsonSerializer.Serialize(feature, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("LocationFeatureSpecification", root.RootElement.GetProperty("@type").GetString());
    }

    // ── DefinedTerm ───────────────────────────────────────────────────────────────

    [Fact]
    public void SchemaOrgDefinedTerm_Serializes_WithIdAndName()
    {
        var term = new SchemaOrgDefinedTerm
        {
            Id = "https://standards.esd.org.uk/?uri=esd%3Aservice%2F841",
            Name = "Day Opportunities",
            InDefinedTermSet = "https://standards.esd.org.uk/"
        };
        var json = JsonSerializer.Serialize(term, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);
        Assert.Equal("DefinedTerm", root.RootElement.GetProperty("@type").GetString());
        Assert.Equal("Day Opportunities", root.RootElement.GetProperty("name").GetString());
        Assert.Equal("https://standards.esd.org.uk/", root.RootElement.GetProperty("inDefinedTermSet").GetString());
    }

    // ── Full service-directory entry roundtrip ────────────────────────────────────

    [Fact]
    public void SchemaOrgDocument_FullServiceEntry_ProducesValidJsonLd()
    {
        var doc = new SchemaOrgDocument
        {
            Graph =
            [
                new SchemaOrgGovernmentService
                {
                    Id = "https://example.gov.uk/services/food-bank",
                    Name = "East District Food Bank",
                    Description = "Weekly food parcels for residents in food poverty.",
                    Provider = new { type = "Organization", id = "https://example.gov.uk/orgs/abc" },
                    Offers =
                    [
                        new SchemaOrgOffer { Price = 0m, PriceCurrency = "GBP" }
                    ],
                    OpeningHoursSpecification =
                    [
                        new SchemaOrgOpeningHoursSpecification
                        {
                            DayOfWeek = SchemaDayOfWeek.Wednesday,
                            Opens = "10:00",
                            Closes = "14:00"
                        }
                    ]
                },
                new SchemaOrgOrganization
                {
                    Id = "https://example.gov.uk/orgs/abc",
                    Name = "ABC Community Charity"
                }
            ]
        };

        var json = JsonSerializer.Serialize(doc, SchemaOrgSerializerOptions.Default);
        using var root = JsonDocument.Parse(json);

        Assert.Equal("https://schema.org", root.RootElement.GetProperty("@context").GetString());
        var graph = root.RootElement.GetProperty("@graph");
        Assert.Equal(2, graph.GetArrayLength());

        var service = graph[0];
        Assert.Equal("GovernmentService", service.GetProperty("@type").GetString());
        Assert.Equal("East District Food Bank", service.GetProperty("name").GetString());

        var offer = service.GetProperty("offers")[0];
        Assert.Equal("Offer", offer.GetProperty("@type").GetString());
        Assert.Equal(0m, offer.GetProperty("price").GetDecimal());
        Assert.Equal("GBP", offer.GetProperty("priceCurrency").GetString());

        var hours = service.GetProperty("openingHoursSpecification")[0];
        Assert.Equal("https://schema.org/Wednesday", hours.GetProperty("dayOfWeek").GetString());

        var org = graph[1];
        Assert.Equal("Organization", org.GetProperty("@type").GetString());
        Assert.Equal("ABC Community Charity", org.GetProperty("name").GetString());
    }
}
