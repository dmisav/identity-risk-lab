using Core.Abstractions;
using Core.Domain;

namespace Core.Rules;

public sealed class ExpiredCredentialRule : IRule
{
    public string RuleId => "expired-credential";

    public int RuleVersion => 1;

    public IReadOnlyList<Finding> Evaluate(Identity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (identity.IsDeleted)
        {
            return [];
        }

        var now = DateTimeOffset.UtcNow;
        return identity.Credentials
            .Where(credential =>
                credential.IsActive &&
                credential.ExpiresAt is not null &&
                credential.ExpiresAt < now)
            .Select(credential => new Finding(
                null,
                identity.Id,
                RuleId,
                RuleVersion,
                credential.Id,
                FindingSeverity.High,
                "Active credential has expired."))
            .ToArray();
    }
}
