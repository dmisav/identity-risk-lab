using Api;

namespace Tests;

public sealed class ApiTests
{
    [Fact]
    public void Resolves_tenant_from_server_side_api_key_mapping()
    {
        var authentication = new TenantApiKeyAuthentication(
            new Dictionary<string, string> { ["key-a"] = "tenant-a" });

        Assert.Equal("tenant-a", authentication.ResolveTenant("key-a"));
    }

    [Fact]
    public void Missing_api_key_is_not_authenticated()
    {
        var authentication = new TenantApiKeyAuthentication(
            new Dictionary<string, string> { ["key-a"] = "tenant-a" });

        Assert.Null(authentication.ResolveTenant(null));
    }

    [Fact]
    public void Findings_are_not_read_across_tenants()
    {
        Assert.Fail("The API findings endpoint skeleton does not expose tenant-scoped reads yet.");
    }
}
