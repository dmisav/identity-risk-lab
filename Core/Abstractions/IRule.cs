using Core.Domain;

namespace Core.Abstractions;

public interface IRule
{
    string RuleId { get; }

    int RuleVersion { get; }

    IReadOnlyList<Finding> Evaluate(Identity identity);
}
