using Core.Abstractions;
using Core.Domain;

namespace Core.Application;

public sealed class IdentityIngestHandler
{
    private readonly IIdentityStore store;
    private readonly IReadOnlyList<IRule> rules;

    public IdentityIngestHandler(IIdentityStore store, IEnumerable<IRule> rules)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.rules = rules?.ToArray() ?? throw new ArgumentNullException(nameof(rules));
    }

    public UpsertResult Handle(string tenantId, IngestEvent ingestEvent)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(ingestEvent);

        var result = store.Apply(tenantId, ingestEvent);
        if (result.Outcome == UpsertOutcome.Ignored)
        {
            return result;
        }

        var findings = rules
            .SelectMany(rule => rule.Evaluate(result.Current))
            .Select(finding => finding with { TenantId = tenantId })
            .ToArray();

        store.ReplaceFindings(tenantId, result.Current.Id, findings);
        return result;
    }
}
