using Core.Domain;

namespace Core.Abstractions;

public enum UpsertOutcome
{
    Inserted,
    Updated,
    Ignored
}

public sealed record UpsertResult(UpsertOutcome Outcome, Identity Current);

public interface IIdentityStore
{
    UpsertResult Apply(string tenantId, IngestEvent ingestEvent);

    Identity? Get(string tenantId, string identityId);

    IReadOnlyList<Finding> GetFindings(string tenantId);

    void ReplaceFindings(
        string tenantId,
        string identityId,
        IReadOnlyList<Finding> findings);
}
