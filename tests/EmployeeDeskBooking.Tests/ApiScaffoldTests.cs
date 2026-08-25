using EmployeeDeskBooking.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeDeskBooking.Tests;

public class ApiScaffoldTests : IClassFixture<WebApplicationFactory<ApiAssemblyMarker>>
{
    private readonly WebApplicationFactory<ApiAssemblyMarker> _factory;

    public ApiScaffoldTests(WebApplicationFactory<ApiAssemblyMarker> factory) => _factory = factory;

    [Fact]
    public async Task Api_health_returns_ok()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
    }
}
