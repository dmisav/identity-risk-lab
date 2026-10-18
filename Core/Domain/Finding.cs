namespace Core.Domain;

public enum FindingSeverity
{
    Low,
    Medium,
    High
}

public sealed record Finding(
    string? TenantId,
    string IdentityId,
    string RuleId,
    int RuleVersion,
    string? CredentialId,
    FindingSeverity Severity,
    string Message);
