using System.Text.Json;
using OrukModels.Models;

namespace OrukModels.Tests;

/// <summary>
/// Tests that deserialise real Bristol Open Place Directory (OPD) fixture data
/// and assert that <see cref="OrukService"/> model properties are correctly populated.
///
/// Fixtures were sourced from:
///   https://bristol.openplace.directory/o/OpenReferralService/v3
/// and are stored in the fixtures/ directory as static JSON files.
/// These tests make no live HTTP calls.
/// </summary>
public class OrukServiceDeserializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string LoadFixture(string fileName)
    {
        var path = Path.Combine("fixtures", fileName);
        return File.ReadAllText(path);
    }

    // ── Single service ────────────────────────────────────────────────────────────

    [Fact]
    public void Deserialize_SingleService_PopulatesId()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("2a3483ea-8966-4f50-9dc3-7bafa1e5c623", service.Id);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesName()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("1625 Independent People", service.Name);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesStatus()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("active", service.Status);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesUrl()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("http://1625ip.co.uk", service.Url);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesEmail()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("enquiries@1625ip.co.uk", service.Email);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesAssuredDate()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("2025-09-02", service.AssuredDate);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesOrganizationId()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal("42d3fa20-b36d-4822-b5dc-68029911caaf", service.OrganizationId);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesOrganizationNavigation()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.NotNull(service.Organization);
        Assert.Equal("42d3fa20-b36d-4822-b5dc-68029911caaf", service.Organization.Id);
        Assert.Equal("1625 Independent People", service.Organization.Name);
    }

    [Fact]
    public void Deserialize_SingleService_OrganizationHasUri()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service?.Organization);
        Assert.Equal("http://1625ip.co.uk", service.Organization.Uri);
    }

    [Fact]
    public void Deserialize_SingleService_OrganizationHasWebsite()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service?.Organization);
        Assert.Equal("http://1625ip.co.uk", service.Organization.Website);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesServiceAreas()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal(2, service.ServiceAreas.Count);
    }

    [Fact]
    public void Deserialize_SingleService_ServiceArea_HasName()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var firstArea = service.ServiceAreas.First();
        Assert.Equal("Bristol", firstArea.Name);
    }

    [Fact]
    public void Deserialize_SingleService_ServiceArea_HasCamelCaseServiceId()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var firstArea = service.ServiceAreas.First();
        Assert.Equal("2a3483ea-8966-4f50-9dc3-7bafa1e5c623", firstArea.ServiceIdCamel);
    }

    [Fact]
    public void Deserialize_SingleService_PopulatesContacts()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Single(service.Contacts);
    }

    [Fact]
    public void Deserialize_SingleService_Contact_HasPhones()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var contact = service.Contacts.First();
        Assert.Equal(2, contact.Phones.Count);
    }

    [Fact]
    public void Deserialize_SingleService_Contact_Phone_HasNumber()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var phone = service.Contacts.First().Phones.First();
        Assert.Equal("0800 0354213", phone.Number);
    }

    [Fact]
    public void Deserialize_SingleService_Extensions_CapturesPcMetadata()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var ext = service.Extensions.SingleOrDefault(e => e.Namespace == "pc" && e.Key == "metadata");
        Assert.NotNull(ext);
        Assert.Equal("pc_metadata", ext.RawKey);
    }

    [Fact]
    public void Deserialize_SingleService_Extensions_PcMetadata_ContainsAssuredBy()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var ext = service.Extensions.Single(e => e.Namespace == "pc" && e.Key == "metadata");
        var assuredBy = ext.Value.GetProperty("assured_by").GetString();
        Assert.Equal("oliviaplenty@thecareforum.org.uk", assuredBy);
    }

    [Fact]
    public void Deserialize_SingleService_Extensions_PcMetadata_ContainsDateCreated()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var ext = service.Extensions.Single(e => e.Namespace == "pc" && e.Key == "metadata");
        var dateCreated = ext.Value.GetProperty("date_created").GetString();
        Assert.Equal("2020-06-05", dateCreated);
    }

    [Fact]
    public void Deserialize_SingleService_Extensions_CapturesPcTargetAudience()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var ext = service.Extensions.SingleOrDefault(e => e.Namespace == "pc" && e.Key == "targetAudience");
        Assert.NotNull(ext);
        Assert.Equal("pc_targetAudience", ext.RawKey);
    }

    [Fact]
    public void Deserialize_SingleService_Extensions_PcTargetAudience_IsArray()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var ext = service.Extensions.Single(e => e.Namespace == "pc" && e.Key == "targetAudience");
        Assert.Equal(JsonValueKind.Array, ext.Value.ValueKind);
        Assert.Equal(2, ext.Value.GetArrayLength());
    }

    [Fact]
    public void Deserialize_SingleService_Extensions_PcTargetAudience_ContainsHomeless()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        var ext = service.Extensions.Single(e => e.Namespace == "pc" && e.Key == "targetAudience");
        var audienceTypes = ext.Value.EnumerateArray()
            .Select(el => el.GetProperty("audienceType").GetString())
            .ToList();
        Assert.Contains("Homeless", audienceTypes);
        Assert.Contains("Care leavers", audienceTypes);
    }

    [Fact]
    public void Deserialize_SingleService_MinimumAge_IsNegativeOneWhenUnset()
    {
        // Bristol OPD uses -1 to indicate "no minimum age constraint"
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.Equal(-1, service.MinimumAge);
    }

    [Fact]
    public void Deserialize_SingleService_LastModified_IsPopulated()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.NotNull(service.LastModified);
        Assert.StartsWith("2025-09-02", service.LastModified);
    }

    [Fact]
    public void Deserialize_SingleService_Description_IsNotEmpty()
    {
        var json = LoadFixture("bristol-service-1625ip.json");
        var service = JsonSerializer.Deserialize<OrukService>(json, Options);
        Assert.NotNull(service);
        Assert.False(string.IsNullOrEmpty(service.Description));
    }

    // ── Paged response ────────────────────────────────────────────────────────────

    [Fact]
    public void Deserialize_PagedResponse_PopulatesTotalItems()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        Assert.True(page.TotalItems > 0);
    }

    [Fact]
    public void Deserialize_PagedResponse_PopulatesTotalPages()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        Assert.True(page.TotalPages > 0);
    }

    [Fact]
    public void Deserialize_PagedResponse_PageNumber_IsOne()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        Assert.Equal(1, page.PageNumber);
    }

    [Fact]
    public void Deserialize_PagedResponse_FirstPage_IsTrue()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        Assert.True(page.FirstPage);
    }

    [Fact]
    public void Deserialize_PagedResponse_Contents_HasExpectedCount()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        Assert.Equal(3, page.Contents.Count);
    }

    [Fact]
    public void Deserialize_PagedResponse_Contents_FirstItem_HasName()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        Assert.NotNull(page.Contents[0].Name);
        Assert.False(string.IsNullOrEmpty(page.Contents[0].Name));
    }

    [Fact]
    public void Deserialize_PagedResponse_Contents_EachItem_HasId()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        foreach (var service in page.Contents)
        {
            Assert.False(string.IsNullOrEmpty(service.Id), $"Service '{service.Name}' has no Id");
        }
    }

    [Fact]
    public void Deserialize_PagedResponse_Contents_EachItem_HasStatus()
    {
        var json = LoadFixture("bristol-services-page1.json");
        var page = JsonSerializer.Deserialize<OrukPage<OrukService>>(json, Options);
        Assert.NotNull(page);
        foreach (var service in page.Contents)
        {
            Assert.False(string.IsNullOrEmpty(service.Status), $"Service '{service.Name}' has no Status");
        }
    }
}
