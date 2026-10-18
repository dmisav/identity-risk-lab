using Api;
using Core.Application;
using Core.Domain;
using Core.Rules;
using Data;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new TenantApiKeyAuthentication(
    new Dictionary<string, string>
    {
        ["key-a"] = "tenant-a",
        ["key-b"] = "tenant-b"
    }));
builder.Services.AddSingleton<InMemoryIdentityStore>();
builder.Services.AddSingleton<IdentityIngestHandler>(services =>
    new IdentityIngestHandler(
        services.GetRequiredService<InMemoryIdentityStore>(),
        [
            new CredentialAgeRule(),
            new MissingOwnerRule(),
            new ExpiredCredentialRule()
        ]));

var app = builder.Build();

app.Use(async (context, next) =>
{
    var authentication = context.RequestServices.GetRequiredService<TenantApiKeyAuthentication>();
    var tenantId = authentication.ResolveTenant(context.Request.Headers["X-Api-Key"].FirstOrDefault());
    if (tenantId is null)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    context.Items[TenantContext.ItemKey] = tenantId;
    await next();
});

app.MapPost(
    "/ingest",
    (HttpContext context, IngestBatchRequest request, IdentityIngestHandler handler) =>
    {
        var tenantId = TenantContext.GetTenant(context);
        var results = request.Events?.Select(eventRequest =>
        {
            if (!IngestEventMapper.TryCreateEvent(eventRequest, out var ingestEvent))
            {
                return new IngestEventResult(eventRequest.IdentityId, "invalid");
            }

            var result = handler.Handle(tenantId, ingestEvent);
            return new IngestEventResult(
                ingestEvent.IdentityId,
                result.Outcome == Core.Abstractions.UpsertOutcome.Ignored
                    ? "ignored_stale"
                    : "applied");
        }).ToArray() ?? [];

        return Results.Accepted(value: new IngestBatchResponse(request.BatchId, results));
    });

app.MapGet(
    "/findings",
    (HttpContext context, InMemoryIdentityStore store, string? severity, int? limit) =>
    {
        if (limit is <= 0)
        {
            return Results.BadRequest(new { error = "limit must be greater than zero." });
        }

        FindingSeverity? requestedSeverity = null;
        if (severity is not null)
        {
            if (!Enum.TryParse<FindingSeverity>(severity, true, out var parsedSeverity))
            {
                return Results.BadRequest(new { error = "severity is invalid." });
            }

            requestedSeverity = parsedSeverity;
        }

        var findings = store.GetFindings(TenantContext.GetTenant(context))
            .Where(finding => requestedSeverity is null || finding.Severity == requestedSeverity)
            .Take(limit ?? 100)
            .ToArray();

        return Results.Ok(findings);
    });

app.Run();

public partial class Program;

internal static class TenantContext
{
    internal const string ItemKey = "tenant-id";

    internal static string GetTenant(HttpContext context) =>
        context.Items[ItemKey] as string
        ?? throw new InvalidOperationException("Tenant context is missing.");
}

internal sealed record IngestBatchRequest(string? BatchId, IReadOnlyList<IngestEventRequest>? Events);

internal sealed record IngestEventRequest(
    string? IdentityId,
    string? Op,
    long? Version,
    JsonElement? Data);

internal sealed record IdentityData(string? Owner, IReadOnlyList<CredentialData>? Credentials);

internal sealed record CredentialData(
    string? Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive);

internal sealed record IngestEventResult(string? IdentityId, string Status);

internal sealed record IngestBatchResponse(
    string? BatchId,
    IReadOnlyList<IngestEventResult> Results);

internal static class IngestEventMapper
{
    internal static bool TryCreateEvent(IngestEventRequest eventRequest, out IngestEvent ingestEvent)
    {
        ingestEvent = default!;
        if (string.IsNullOrWhiteSpace(eventRequest.IdentityId) || eventRequest.Version is null)
        {
            return false;
        }

        if (string.Equals(eventRequest.Op, "delete", StringComparison.OrdinalIgnoreCase))
        {
            ingestEvent = new IngestEvent(
                eventRequest.IdentityId,
                IngestOperation.Delete,
                eventRequest.Version.Value,
                null);
            return true;
        }

        if (!string.Equals(eventRequest.Op, "upsert", StringComparison.OrdinalIgnoreCase) ||
            eventRequest.Data is not { ValueKind: JsonValueKind.Object } data)
        {
            return false;
        }

        try
        {
            var identityData = JsonSerializer.Deserialize<IdentityData>(data.GetRawText());
            if (identityData?.Credentials?.Any(credential =>
                    credential is null ||
                    string.IsNullOrWhiteSpace(credential.Id) ||
                    credential.CreatedAt == default) == true)
            {
                return false;
            }

            var credentials = identityData?.Credentials?
                .Select(credential => new Credential(
                    credential.Id!,
                    credential.CreatedAt,
                    credential.ExpiresAt,
                    credential.IsActive))
                .ToArray() ?? [];
            ingestEvent = new IngestEvent(
                eventRequest.IdentityId,
                IngestOperation.Upsert,
                eventRequest.Version.Value,
                new Identity(
                    eventRequest.IdentityId,
                    identityData?.Owner,
                    credentials,
                    eventRequest.Version.Value,
                    false));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
