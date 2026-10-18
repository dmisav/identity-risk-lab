namespace Core.Domain;

public sealed record Credential(
    string Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive);
