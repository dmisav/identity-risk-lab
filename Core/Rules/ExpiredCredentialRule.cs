using Core.Abstractions;
using Core.Domain;

namespace Core.Rules;

public sealed class ExpiredCredentialRule : IRule
{
    public string RuleId => "expired-credential";

    public int RuleVersion => 1;

    public Finding? Evaluate(string tenantId, Identity identity)
    {
        throw new NotImplementedException();
    }
}
