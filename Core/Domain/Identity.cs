namespace Core.Domain;

public sealed record Identity(
    string Id,
    string? Owner,
    IReadOnlyList<Credential> Credentials,
    long Version,
    bool IsDeleted);
