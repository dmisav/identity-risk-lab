using Core.Domain;

namespace Core.Abstractions;

public interface IRule
{
    string RuleId { get; }

    int RuleVersion { get; }

    Finding? Evaluate(string tenantId, Identity identity);
}
