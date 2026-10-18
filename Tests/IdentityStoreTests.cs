using Core.Abstractions;
using Core.Domain;
using Data;

namespace Tests;

public sealed class IdentityStoreTests
{
    [Fact]
    public void Insert_new_identity_is_applied()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        var result = store.Apply("tenant-a", Event("identity-1", 1));

        Assert.Equal(UpsertOutcome.Inserted, result.Outcome);
        Assert.Equal("identity-1", store.Get("tenant-a", "identity-1")!.Id);
    }

    [Fact]
    public void Newer_version_replaces_the_stored_one()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 1, owner: "old-owner"));
        var result = store.Apply("tenant-a", Event("identity-1", 2, owner: "new-owner"));

        Assert.Equal(UpsertOutcome.Updated, result.Outcome);
        Assert.Equal(2, store.Get("tenant-a", "identity-1")!.Version);
        Assert.Equal("new-owner", store.Get("tenant-a", "identity-1")!.Owner);
    }

    [Fact]
    public void Older_version_is_ignored()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 2, owner: "current-owner"));
        var result = store.Apply("tenant-a", Event("identity-1", 1, owner: "old-owner"));

        Assert.Equal(UpsertOutcome.Ignored, result.Outcome);
        Assert.Equal(2, store.Get("tenant-a", "identity-1")!.Version);
        Assert.Equal("current-owner", store.Get("tenant-a", "identity-1")!.Owner);
    }

    [Fact]
    public void Equal_version_is_ignored()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 1, owner: "current-owner"));
        var result = store.Apply("tenant-a", Event("identity-1", 1, owner: "different-owner"));

        Assert.Equal(UpsertOutcome.Ignored, result.Outcome);
        Assert.Equal("current-owner", store.Get("tenant-a", "identity-1")!.Owner);
    }

    [Fact]
    public void Delete_leaves_tombstone_and_late_lower_version_create_is_ignored()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 2, IngestOperation.Delete));
        var result = store.Apply("tenant-a", Event("identity-1", 1));

        Assert.Equal(UpsertOutcome.Ignored, result.Outcome);
        var identity = store.Get("tenant-a", "identity-1")!;
        Assert.True(identity.IsDeleted);
        Assert.Equal(2, identity.Version);
    }

    [Fact]
    public void Tenant_isolation_keeps_same_identity_ids_separate()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 1, owner: "tenant-a-owner"));
        store.Apply("tenant-b", Event("identity-1", 1, owner: "tenant-b-owner"));

        Assert.Equal("tenant-a-owner", store.Get("tenant-a", "identity-1")!.Owner);
        Assert.Equal("tenant-b-owner", store.Get("tenant-b", "identity-1")!.Owner);
    }

    [Fact]
    public void Reading_as_tenant_b_never_returns_tenant_a_data()
    {
        IIdentityStore store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 1, owner: "tenant-a-owner"));

        Assert.Null(store.Get("tenant-b", "identity-1"));
    }

    private static IngestEvent Event(
        string identityId,
        long version,
        IngestOperation operation = IngestOperation.Upsert,
        string? owner = "owner") =>
        new(
            identityId,
            operation,
            version,
            operation == IngestOperation.Upsert
                ? new Identity(identityId, owner, [], version, false)
                : null);
}
