using System.Net;
using System.Net.Http.Json;
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

    private static string MakePage(
        int pageNumber, int totalPages, int totalItems, IEnumerable<OrukService> services)
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
            contents = services
        };
        return JsonSerializer.Serialize(page);
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
}
