using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using OrukModels.Models;
using OrukTransformer.Cli.Fetching;
using RichardSzalay.MockHttp;

namespace OrukTransformer.Cli.Tests;

/// <summary>
/// Tests for <see cref="OrukFeedPageFetcher"/>.
/// </summary>
public class OrukFeedPageFetcherTests
{
    private static readonly Uri BaseUrl = new("https://example.org/services");

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions OmitNullsOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static string MakePage(
        int pageNumber, int totalPages, int totalItems, IEnumerable<OrukService> services,
        string? nextUrl = null)
    {
        var page = new
        {
            total_items = totalItems,
            total_pages = totalPages,
            page_number = pageNumber,
            size = services.Count(),
            first_page = pageNumber == 1,
            last_page = pageNumber == totalPages,
            empty = false,
            contents = services,
            next_url = nextUrl
        };
        return JsonSerializer.Serialize(page, OmitNullsOptions);
    }

    private static string MakeRdpePage(IEnumerable<OrukService> services, string? nextUrl = null,
        bool useCamelCase = false)
    {
        // Produces a minimal RDPE-style response (no total_pages etc.)
        if (useCamelCase)
        {
            var page = new { contents = services, nextUrl };
            return JsonSerializer.Serialize(page, OmitNullsOptions);
        }
        else
        {
            var page = new { contents = services, next_url = nextUrl };
            return JsonSerializer.Serialize(page, OmitNullsOptions);
        }
    }

    private static OrukService MakeService(string id) => new() { Id = id, Name = $"Service {id}" };

    private static (MockHttpMessageHandler mock, HttpClient client) SetupHttpClient()
    {
        var mock = new MockHttpMessageHandler();
        var client = mock.ToHttpClient();
        return (mock, client);
    }

    private static OrukFeedPageFetcher MakeFetcher(HttpClient client) =>
        new(client, NullLogger<OrukFeedPageFetcher>.Instance);

    // ── Tests ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FetchAsync_SinglePage_ReturnsAllServices()
    {
        var (mock, client) = SetupHttpClient();
        var services = new[] { MakeService("1"), MakeService("2"), MakeService("3") };
        var json = MakePage(1, 1, 3, services);

        mock.When("*").Respond("application/json", json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 50))
            result.Add(s);

        Assert.Equal(3, result.Count);
        Assert.Equal(["1", "2", "3"], result.Select(s => s.Id));
    }

    [Fact]
    public async Task FetchAsync_MultiplePages_FetchesAllPages()
    {
        var (mock, client) = SetupHttpClient();
        var page1Json = MakePage(1, 2, 4, [MakeService("1"), MakeService("2")]);
        var page2Json = MakePage(2, 2, 4, [MakeService("3"), MakeService("4")]);

        // Use a handler that returns different responses based on page query param
        mock.When(HttpMethod.Get, "https://example.org/services")
            .WithQueryString("page", "1")
            .Respond("application/json", page1Json);
        mock.When(HttpMethod.Get, "https://example.org/services")
            .WithQueryString("page", "2")
            .Respond("application/json", page2Json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 0))
            result.Add(s);

        Assert.Equal(4, result.Count);
        Assert.Equal(["1", "2", "3", "4"], result.Select(s => s.Id));
    }

    [Fact]
    public async Task FetchAsync_MaxRecordsReached_StopsEarly()
    {
        var (mock, client) = SetupHttpClient();
        // 10 services across 2 pages, but we only want 3
        var page1Json = MakePage(1, 2, 10,
            Enumerable.Range(1, 5).Select(i => MakeService(i.ToString())));

        mock.When("*").Respond("application/json", page1Json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 3))
            result.Add(s);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task FetchAsync_MaxRecordsZero_FetchesAllServices()
    {
        var (mock, client) = SetupHttpClient();
        var services = Enumerable.Range(1, 10).Select(i => MakeService(i.ToString()));
        var json = MakePage(1, 1, 10, services);

        mock.When("*").Respond("application/json", json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 0))
            result.Add(s);

        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task FetchAsync_NegativeMaxRecords_FetchesAllServices()
    {
        var (mock, client) = SetupHttpClient();
        var services = Enumerable.Range(1, 5).Select(i => MakeService(i.ToString()));
        var json = MakePage(1, 1, 5, services);

        mock.When("*").Respond("application/json", json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: -1))
            result.Add(s);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task FetchAsync_FirstPageHttpError_ReturnsEmpty()
    {
        var (mock, client) = SetupHttpClient();
        mock.When("*").Respond(HttpStatusCode.InternalServerError);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 50))
            result.Add(s);

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAsync_SecondPageHttpError_ReturnsFirstPageServices()
    {
        var (mock, client) = SetupHttpClient();
        var page1Json = MakePage(1, 2, 4, [MakeService("1"), MakeService("2")]);

        mock.When(HttpMethod.Get, "https://example.org/services")
            .WithQueryString("page", "1")
            .Respond("application/json", page1Json);
        mock.When(HttpMethod.Get, "https://example.org/services")
            .WithQueryString("page", "2")
            .Respond(HttpStatusCode.ServiceUnavailable);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 0))
            result.Add(s);

        // Should still return page 1 services despite page 2 failure
        Assert.Equal(2, result.Count);
        Assert.Equal(["1", "2"], result.Select(s => s.Id));
    }

    [Fact]
    public async Task FetchAsync_PageUrlContainsPageAndPerPage()
    {
        var (mock, client) = SetupHttpClient();
        var capturedUrls = new List<string>();

        var services = new[] { MakeService("1") };
        var json = MakePage(1, 1, 1, services);

        mock.When("*").Respond(req =>
        {
            capturedUrls.Add(req.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var fetcher = MakeFetcher(client);
        await foreach (var _ in fetcher.FetchAsync(BaseUrl, maxRecords: 10)) { }

        Assert.Single(capturedUrls);
        Assert.Contains("page=1", capturedUrls[0]);
        Assert.Contains("per_page=10", capturedUrls[0]);
    }

    [Fact]
    public async Task FetchAsync_LargeMaxRecords_PerPageCappedAt100()
    {
        var (mock, client) = SetupHttpClient();
        var capturedUrls = new List<string>();
        var services = new[] { MakeService("1") };
        var json = MakePage(1, 1, 1, services);

        mock.When("*").Respond(req =>
        {
            capturedUrls.Add(req.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var fetcher = MakeFetcher(client);
        await foreach (var _ in fetcher.FetchAsync(BaseUrl, maxRecords: 500)) { }

        Assert.Single(capturedUrls);
        Assert.Contains("per_page=100", capturedUrls[0]);
    }

    // ── RDPE paging tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task FetchAsync_RdpeSnakeCaseNextUrl_FollowsAllPages()
    {
        // Arrange: three RDPE pages using snake_case "next_url"
        var (mock, client) = SetupHttpClient();
        const string page2Url = "https://example.org/services?afterTimestamp=100&afterId=s2";
        const string page3Url = "https://example.org/services?afterTimestamp=200&afterId=s4";

        var page1Json = MakeRdpePage([MakeService("1"), MakeService("2")], nextUrl: page2Url);
        var page2Json = MakeRdpePage([MakeService("3"), MakeService("4")], nextUrl: page3Url);
        var page3Json = MakeRdpePage([MakeService("5")], nextUrl: null);  // last page

        mock.When(HttpMethod.Get, "https://example.org/services")
            .WithQueryString("page", "1")
            .Respond("application/json", page1Json);
        mock.When(HttpMethod.Get, page2Url)
            .Respond("application/json", page2Json);
        mock.When(HttpMethod.Get, page3Url)
            .Respond("application/json", page3Json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 0))
            result.Add(s);

        Assert.Equal(5, result.Count);
        Assert.Equal(["1", "2", "3", "4", "5"], result.Select(s => s.Id));
    }

    [Fact]
    public async Task FetchAsync_RdpeCamelCaseNextUrl_FollowsAllPages()
    {
        // Arrange: two RDPE pages using camelCase "nextUrl" (as returned by some feeds)
        var (mock, client) = SetupHttpClient();
        const string page2Url = "https://example.org/services?afterTimestamp=50&afterId=svc1";

        var page1Json = MakeRdpePage([MakeService("A"), MakeService("B")],
            nextUrl: page2Url, useCamelCase: true);
        var page2Json = MakeRdpePage([MakeService("C")], nextUrl: null, useCamelCase: true);

        mock.When(HttpMethod.Get, "https://example.org/services")
            .WithQueryString("page", "1")
            .Respond("application/json", page1Json);
        mock.When(HttpMethod.Get, page2Url)
            .Respond("application/json", page2Json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 0))
            result.Add(s);

        Assert.Equal(3, result.Count);
        Assert.Equal(["A", "B", "C"], result.Select(s => s.Id));
    }

    [Fact]
    public async Task FetchAsync_RdpeSinglePage_NoNextUrl_ReturnsServicesAndStops()
    {
        var (mock, client) = SetupHttpClient();
        var json = MakeRdpePage([MakeService("X"), MakeService("Y")], nextUrl: null);

        mock.When("*").Respond("application/json", json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 0))
            result.Add(s);

        Assert.Equal(2, result.Count);
        Assert.Equal(["X", "Y"], result.Select(s => s.Id));
    }

    [Fact]
    public async Task FetchAsync_RdpeCircularNextUrl_StopsPagination()
    {
        // A feed returning the same URL as next_url signals "up to date".
        // The fetcher should stop after visiting that URL once (no infinite loop).
        var (mock, client) = SetupHttpClient();
        const string selfUrl = "https://example.org/services?afterTimestamp=0&afterId=last";

        // The mock returns the same payload for any URL (simulating an endpoint that always
        // points back to itself as the "up to date" indicator).
        var json = MakeRdpePage([MakeService("1")], nextUrl: selfUrl);
        mock.When("*").Respond("application/json", json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(new Uri(selfUrl), maxRecords: 0))
            result.Add(s);

        // Two fetches occur: the initial request (with page params) and one visit to selfUrl.
        // Once selfUrl's response also points to selfUrl (circular), pagination stops.
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FetchAsync_RdpeMaxRecordsReached_StopsEarly()
    {
        // RDPE feed returns many pages but we only want 3 records.
        var (mock, client) = SetupHttpClient();
        const string page2Url = "https://example.org/services?afterTimestamp=100&afterId=s2";

        var page1Json = MakeRdpePage(
            Enumerable.Range(1, 5).Select(i => MakeService(i.ToString())),
            nextUrl: page2Url);

        mock.When("*").Respond("application/json", page1Json);

        var fetcher = MakeFetcher(client);
        var result = new List<OrukService>();
        await foreach (var s in fetcher.FetchAsync(BaseUrl, maxRecords: 3))
            result.Add(s);

        Assert.Equal(3, result.Count);
    }
}
