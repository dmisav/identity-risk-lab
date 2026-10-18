using Api;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests;

public sealed class ApiTests
{
    [Fact]
    public void Resolves_tenant_from_server_side_api_key_mapping()
    {
        var authentication = new TenantApiKeyAuthentication(
            new Dictionary<string, string> { ["key-a"] = "tenant-a" });

        Assert.Equal("tenant-a", authentication.ResolveTenant("key-a"));
    }

    [Fact]
    public void Missing_api_key_is_not_authenticated()
    {
        var authentication = new TenantApiKeyAuthentication(
            new Dictionary<string, string> { ["key-a"] = "tenant-a" });

        Assert.Null(authentication.ResolveTenant(null));
    }

    [Fact]
    public async Task Missing_api_key_returns_unauthorized()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/findings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_a_cannot_see_tenant_b_findings()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await PostBatch(
            client,
            "key-b",
            "batch-b",
            new EventRequest("identity-1", "upsert", 1, new { owner = (string?)null, credentials = Array.Empty<object>() }));

        var response = await GetFindings(client, "key-a");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await Findings(response));
    }

    [Fact]
    public async Task Tenant_id_in_request_body_is_ignored()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await PostBatch(
            client,
            "key-a",
            "batch-a",
            [new EventRequest("identity-1", "upsert", 1, new { owner = (string?)null, credentials = Array.Empty<object>() })],
            tenantId: "tenant-b");

        var tenantAResponse = await GetFindings(client, "key-a");
        var tenantBResponse = await GetFindings(client, "key-b");

        Assert.Single(await Findings(tenantAResponse));
        Assert.Empty(await Findings(tenantBResponse));
    }

    [Fact]
    public async Task Reposting_the_same_batch_is_idempotent()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var request = new EventRequest(
            "identity-1",
            "upsert",
            1,
            new { owner = (string?)null, credentials = Array.Empty<object>() });

        await PostBatch(client, "key-a", "batch-a", request);
        await PostBatch(client, "key-a", "batch-a", request);

        var response = await GetFindings(client, "key-a");

        Assert.Single(await Findings(response));
    }

    [Fact]
    public async Task Stale_event_is_reported_as_ignored_stale()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await PostBatch(
            client,
            "key-a",
            "batch-new",
            new EventRequest("identity-1", "upsert", 2, new { owner = "owner", credentials = Array.Empty<object>() }));
        var response = await PostBatch(
            client,
            "key-a",
            "batch-stale",
            new EventRequest("identity-1", "upsert", 1, new { owner = "owner", credentials = Array.Empty<object>() }));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal("ignored_stale", (await Results(response))[0]);
    }

    [Fact]
    public async Task Malformed_event_does_not_block_other_events()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await PostBatch(
            client,
            "key-a",
            "batch-a",
            new EventRequest("invalid", "unsupported", 1, new { owner = "owner", credentials = Array.Empty<object>() }),
            new EventRequest("valid", "upsert", 1, new { owner = (string?)null, credentials = Array.Empty<object>() }));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(new[] { "invalid", "applied" }, await Results(response));

        var findingsResponse = await GetFindings(client, "key-a");
        Assert.Single(await Findings(findingsResponse));
    }

    private static async Task<HttpResponseMessage> PostBatch(
        HttpClient client,
        string apiKey,
        string batchId,
        params EventRequest[] events) =>
        await PostBatch(client, apiKey, batchId, events, tenantId: null);

    private static async Task<HttpResponseMessage> PostBatch(
        HttpClient client,
        string apiKey,
        string batchId,
        EventRequest[] events,
        string? tenantId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/ingest")
        {
            Content = JsonContent.Create(new { batchId, tenantId, events })
        };
        request.Headers.Add("X-Api-Key", apiKey);

        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> GetFindings(HttpClient client, string apiKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/findings");
        request.Headers.Add("X-Api-Key", apiKey);

        return await client.SendAsync(request);
    }

    private static async Task<string[]> Results(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement
            .GetProperty("results")
            .EnumerateArray()
            .Select(result => result.GetProperty("status").GetString()!)
            .ToArray();
    }

    private static async Task<JsonElement[]> Findings(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.EnumerateArray().ToArray();
    }

    private sealed record EventRequest(string IdentityId, string Op, long Version, object Data);
}
