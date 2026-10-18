using Core.Abstractions;
using Core.Domain;

namespace Data;

public sealed class InMemoryIdentityStore : IIdentityStore
{
    public UpsertResult Apply(string tenantId, IngestEvent ingestEvent)
    {
        throw new NotImplementedException();
    }

    public Identity? Get(string tenantId, string identityId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<Finding> GetFindings(string tenantId)
    {
        throw new NotImplementedException();
    }

    public void ReplaceFindings(
        string tenantId,
        string identityId,
        IReadOnlyList<Finding> findings)
    {
        throw new NotImplementedException();
    }
}
