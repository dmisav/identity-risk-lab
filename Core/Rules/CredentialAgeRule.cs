using Core.Abstractions;
using Core.Domain;

namespace Core.Rules;

public sealed class CredentialAgeRule : IRule
{
    public string RuleId => "credential-age";

    public int RuleVersion => 1;

    public IReadOnlyList<Finding> Evaluate(Identity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (identity.IsDeleted)
        {
            return [];
        }

        var threshold = DateTimeOffset.UtcNow.AddDays(-90);
        return identity.Credentials
            .Where(credential => credential.CreatedAt < threshold)
            .Select(credential => new Finding(
                null,
                identity.Id,
                RuleId,
                RuleVersion,
                credential.Id,
                FindingSeverity.Medium,
                "Credential is older than 90 days."))
            .ToArray();
    }
}
