using Core.Domain;
using Core.Rules;

namespace Tests;

public sealed class RuleTests
{
    [Fact]
    public void Flags_credential_older_than_90_days()
    {
        var credential = new Credential(
            "credential-1",
            DateTimeOffset.UtcNow.AddDays(-91),
            DateTimeOffset.UtcNow.AddDays(30),
            true);
        var identity = new Identity("identity-1", "owner", [credential], 1, false);

        var finding = new CredentialAgeRule().Evaluate("tenant-a", identity);

        Assert.NotNull(finding);
        Assert.Equal("credential-age", finding.RuleId);
        Assert.Equal(1, finding.RuleVersion);
    }

    [Fact]
    public void Flags_identity_with_no_owner()
    {
        var identity = new Identity("identity-1", null, [], 1, false);

        var finding = new MissingOwnerRule().Evaluate("tenant-a", identity);

        Assert.NotNull(finding);
        Assert.Equal("missing-owner", finding.RuleId);
        Assert.Equal(1, finding.RuleVersion);
    }

    [Fact]
    public void Flags_active_credential_that_is_expired()
    {
        var credential = new Credential(
            "credential-1",
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow.AddDays(-1),
            true);
        var identity = new Identity("identity-1", "owner", [credential], 1, false);

        var finding = new ExpiredCredentialRule().Evaluate("tenant-a", identity);

        Assert.NotNull(finding);
        Assert.Equal("expired-credential", finding.RuleId);
        Assert.Equal(1, finding.RuleVersion);
    }
}
