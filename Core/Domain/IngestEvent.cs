namespace Core.Domain;

public enum IngestOperation
{
    Upsert,
    Delete
}

public sealed record IngestEvent(
    string IdentityId,
    IngestOperation Operation,
    long Version,
    Identity? Identity);
