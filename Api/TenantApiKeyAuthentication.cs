namespace Api;

public sealed class TenantApiKeyAuthentication
{
    private readonly IReadOnlyDictionary<string, string> apiKeys;

    public TenantApiKeyAuthentication(IReadOnlyDictionary<string, string> apiKeys)
    {
        this.apiKeys = apiKeys ?? throw new ArgumentNullException(nameof(apiKeys));
    }

    public string? ResolveTenant(string? apiKey)
    {
        return apiKey is not null && apiKeys.TryGetValue(apiKey, out var tenantId)
            ? tenantId
            : null;
    }
}
