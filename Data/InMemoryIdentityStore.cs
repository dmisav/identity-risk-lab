using Core.Abstractions;
using Core.Domain;

namespace Data;

public sealed class InMemoryIdentityStore : IIdentityStore
{
    private readonly object sync = new();
    private readonly Dictionary<(string TenantId, string IdentityId), Identity> identities = new();
    private readonly Dictionary<string, List<Finding>> findings = new();

    public UpsertResult Apply(string tenantId, IngestEvent ingestEvent)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(ingestEvent);

        lock (sync)
        {
            var key = (tenantId, ingestEvent.IdentityId);
            identities.TryGetValue(key, out var current);

            if (current is not null && ingestEvent.Version <= current.Version)
            {
                return new UpsertResult(UpsertOutcome.Ignored, current);
            }

            var next = ingestEvent.Operation == IngestOperation.Delete
                ? new Identity(ingestEvent.IdentityId, null, [], ingestEvent.Version, true)
                : ingestEvent.Identity
                    ?? throw new ArgumentException(
                        "An upsert event must include an identity.",
                        nameof(ingestEvent));

            identities[key] = next;
            return new UpsertResult(
                current is null ? UpsertOutcome.Inserted : UpsertOutcome.Updated,
                next);
        }
    }

    public Identity? Get(string tenantId, string identityId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(identityId);

        lock (sync)
        {
            identities.TryGetValue((tenantId, identityId), out var identity);
            return identity;
        }
    }

    public IReadOnlyList<Finding> GetFindings(string tenantId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        lock (sync)
        {
            return findings.TryGetValue(tenantId, out var tenantFindings)
                ? tenantFindings.ToArray()
                : [];
        }
    }

    public void ReplaceFindings(
        string tenantId,
        string identityId,
        IReadOnlyList<Finding> findings)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(identityId);
        ArgumentNullException.ThrowIfNull(findings);

        lock (sync)
        {
            if (!this.findings.TryGetValue(tenantId, out var tenantFindings))
            {
                tenantFindings = [];
                this.findings[tenantId] = tenantFindings;
            }

            tenantFindings.RemoveAll(finding => finding.IdentityId == identityId);
            tenantFindings.AddRange(findings);
        }
    }
}
