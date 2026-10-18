using Core.Abstractions;
using Core.Domain;

namespace Core.Rules;

public sealed class MissingOwnerRule : IRule
{
    public string RuleId => "missing-owner";

    public int RuleVersion => 1;

    public IReadOnlyList<Finding> Evaluate(Identity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return identity.IsDeleted || !string.IsNullOrWhiteSpace(identity.Owner)
            ? []
            :
            [
                new Finding(
                    null,
                    identity.Id,
                    RuleId,
                    RuleVersion,
                    null,
                    FindingSeverity.Medium,
                    "Identity has no owner.")
            ];
    }
}
