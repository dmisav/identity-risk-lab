namespace Core.Domain;

public sealed record Finding(
    string TenantId,
    string IdentityId,
    string RuleId,
    int RuleVersion,
    string? CredentialId,
    string Message);
