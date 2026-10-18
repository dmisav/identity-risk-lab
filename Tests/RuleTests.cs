using Core.Domain;
using Core.Rules;

namespace Tests;

public sealed class RuleTests
{
    [Fact]
    public void Credential_older_than_90_days_creates_finding()
    {
        var credential = new Credential(
            "credential-1",
            DateTimeOffset.UtcNow.AddDays(-91),
            DateTimeOffset.UtcNow.AddDays(30),
            true);
        var identity = new Identity("identity-1", "owner", [credential], 1, false);

        var findings = new CredentialAgeRule().Evaluate(identity);

        var finding = Assert.Single(findings);
        Assert.Equal("credential-age", finding.RuleId);
        Assert.Equal(1, finding.RuleVersion);
        Assert.Equal("credential-1", finding.CredentialId);
        Assert.Equal(FindingSeverity.Medium, finding.Severity);
    }

    [Fact]
    public void Credential_not_older_than_90_days_creates_no_finding()
    {
        var credential = new Credential(
            "credential-1",
            DateTimeOffset.UtcNow.AddDays(-89),
            DateTimeOffset.UtcNow.AddDays(30),
            true);
        var identity = new Identity("identity-1", "owner", [credential], 1, false);

        var findings = new CredentialAgeRule().Evaluate(identity);

        Assert.Empty(findings);
    }

    [Fact]
    public void Identity_with_no_owner_creates_finding()
    {
        var identity = new Identity("identity-1", null, [], 1, false);

        var findings = new MissingOwnerRule().Evaluate(identity);

        var finding = Assert.Single(findings);
        Assert.Equal("missing-owner", finding.RuleId);
        Assert.Equal(1, finding.RuleVersion);
        Assert.Null(finding.CredentialId);
        Assert.Equal(FindingSeverity.Medium, finding.Severity);
    }

    [Fact]
    public void Identity_with_owner_creates_no_finding()
    {
        var identity = new Identity("identity-1", "owner", [], 1, false);

        var findings = new MissingOwnerRule().Evaluate(identity);

        Assert.Empty(findings);
    }

    [Fact]
    public void Active_expired_credential_creates_finding()
    {
        var credential = new Credential(
            "credential-1",
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow.AddDays(-1),
            true);
        var identity = new Identity("identity-1", "owner", [credential], 1, false);

        var findings = new ExpiredCredentialRule().Evaluate(identity);

        var finding = Assert.Single(findings);
        Assert.Equal("expired-credential", finding.RuleId);
        Assert.Equal(1, finding.RuleVersion);
        Assert.Equal("credential-1", finding.CredentialId);
        Assert.Equal(FindingSeverity.High, finding.Severity);
    }

    [Fact]
    public void Inactive_expired_credential_creates_no_finding()
    {
        var credential = new Credential(
            "credential-1",
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow.AddDays(-1),
            false);
        var identity = new Identity("identity-1", "owner", [credential], 1, false);

        var findings = new ExpiredCredentialRule().Evaluate(identity);

        Assert.Empty(findings);
    }
}
