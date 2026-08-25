using EmployeeDeskBooking.Web;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeDeskBooking.Tests;

public class WebScaffoldTests : IClassFixture<WebApplicationFactory<WebAssemblyMarker>>
{
    private readonly WebApplicationFactory<WebAssemblyMarker> _factory;

    public WebScaffoldTests(WebApplicationFactory<WebAssemblyMarker> factory) => _factory = factory;

    [Fact]
    public async Task Web_home_returns_ok()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}
