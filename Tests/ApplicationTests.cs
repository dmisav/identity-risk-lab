using Core.Application;
using Core.Domain;
using Data;

namespace Tests;

public sealed class ApplicationTests
{
    [Fact]
    public void Stale_ingest_does_not_replace_current_identity()
    {
        var handler = new IdentityIngestHandler(new InMemoryIdentityStore(), []);

        handler.Handle("tenant-a", Event(2));
        handler.Handle("tenant-a", Event(1));

        Assert.Fail("The ingest handler skeleton does not expose its resulting identity yet.");
    }

    private static IngestEvent Event(long version) =>
        new(
            "identity-1",
            IngestOperation.Upsert,
            version,
            new Identity("identity-1", "owner", [], version, false));
}
