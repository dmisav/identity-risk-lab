using Core.Abstractions;
using Core.Domain;

namespace Core.Rules;

public sealed class MissingOwnerRule : IRule
{
    public string RuleId => "missing-owner";

    public int RuleVersion => 1;

    public Finding? Evaluate(string tenantId, Identity identity)
    {
        throw new NotImplementedException();
    }
}
