using Core.Application;
using Core.Domain;
using Core.Rules;
using Data;

namespace Tests;

public sealed class ApplicationTests
{
    [Fact]
    public void Stale_ingest_creates_no_new_findings()
    {
        var store = new InMemoryIdentityStore();
        var handler = new IdentityIngestHandler(store, [new MissingOwnerRule()]);

        handler.Handle("tenant-a", Event(2, owner: null));
        handler.Handle("tenant-a", Event(1, owner: "owner"));

        var findings = store.GetFindings("tenant-a");
        Assert.Single(findings);
        Assert.Equal("missing-owner", findings[0].RuleId);
    }

    [Fact]
    public void Repeated_ingest_leaves_a_single_finding()
    {
        var store = new InMemoryIdentityStore();
        var handler = new IdentityIngestHandler(store, [new MissingOwnerRule()]);

        handler.Handle("tenant-a", Event(1, owner: null));
        handler.Handle("tenant-a", Event(1, owner: null));

        var findings = store.GetFindings("tenant-a");
        Assert.Single(findings);
        Assert.Equal("tenant-a", findings[0].TenantId);
    }

    private static IngestEvent Event(long version, string? owner) =>
        new(
            "identity-1",
            IngestOperation.Upsert,
            version,
            new Identity("identity-1", owner, [], version, false));
}
