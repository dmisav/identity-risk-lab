using Core.Domain;
using Data;

namespace Tests;

public sealed class IdentityStoreTests
{
    [Fact]
    public void Applies_newer_version()
    {
        var store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 1));
        store.Apply("tenant-a", Event("identity-1", 2));

        Assert.Equal(2, store.Get("tenant-a", "identity-1")!.Version);
    }

    [Fact]
    public void Ignores_older_version()
    {
        var store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 2));
        store.Apply("tenant-a", Event("identity-1", 1));

        Assert.Equal(2, store.Get("tenant-a", "identity-1")!.Version);
    }

    [Fact]
    public void Equal_version_is_idempotent()
    {
        var store = new InMemoryIdentityStore();

        var first = store.Apply("tenant-a", Event("identity-1", 1));
        var second = store.Apply("tenant-a", Event("identity-1", 1));

        Assert.Equal(first.Current, second.Current);
    }

    [Fact]
    public void Delete_then_late_create_keeps_tombstone()
    {
        var store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 2, IngestOperation.Delete));
        store.Apply("tenant-a", Event("identity-1", 1));

        var identity = store.Get("tenant-a", "identity-1")!;
        Assert.True(identity.IsDeleted);
        Assert.Equal(2, identity.Version);
    }

    [Fact]
    public void Isolates_tenants()
    {
        var store = new InMemoryIdentityStore();

        store.Apply("tenant-a", Event("identity-1", 1));

        Assert.Null(store.Get("tenant-b", "identity-1"));
    }

    private static IngestEvent Event(
        string identityId,
        long version,
        IngestOperation operation = IngestOperation.Upsert) =>
        new(
            identityId,
            operation,
            version,
            operation == IngestOperation.Upsert
                ? new Identity(identityId, "owner", [], version, false)
                : null);
}
