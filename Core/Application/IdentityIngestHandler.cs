using Core.Abstractions;
using Core.Domain;

namespace Core.Application;

public sealed class IdentityIngestHandler
{
    public IdentityIngestHandler(IIdentityStore store, IEnumerable<IRule> rules)
    {
        throw new NotImplementedException();
    }

    public UpsertResult Handle(string tenantId, IngestEvent ingestEvent)
    {
        throw new NotImplementedException();
    }
}
