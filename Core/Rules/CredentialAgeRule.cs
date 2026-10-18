using Core.Abstractions;
using Core.Domain;

namespace Core.Rules;

public sealed class CredentialAgeRule : IRule
{
    public string RuleId => "credential-age";

    public int RuleVersion => 1;

    public Finding? Evaluate(string tenantId, Identity identity)
    {
        throw new NotImplementedException();
    }
}
