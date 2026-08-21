using AngleSharp;
using AngleSharp.Html.Dom;

namespace EmployeeDeskBooking.Tests;

public sealed class AdminDesksTestClient(CustomWebApplicationFactory factory)
{
    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public LoginTestClient Login { get; } = factory.CreateLoginTestClient();

    public async Task LoginAsAdminAsync()
    {
        var response = await Login.LoginAsync("admin@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(302, (int)response.StatusCode);
    }

    public Task<HttpResponseMessage> GetAdminDesksPageAsync() =>
        Login.Client.GetAsync("/Admin/AdminDesks");

    public async Task<HttpResponseMessage> CreateDeskAsync(string deskNumber)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["deskNumber"] = deskNumber,
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminDesks/Create", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> EditDeskAsync(Guid deskId, string deskNumber)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["deskId"] = deskId.ToString(),
            ["deskNumber"] = deskNumber,
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminDesks/Edit", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> DeactivateDeskAsync(Guid deskId)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["deskId"] = deskId.ToString(),
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminDesks/Deactivate", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> ActivateDeskAsync(Guid deskId)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["deskId"] = deskId.ToString(),
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminDesks/Activate", new FormUrlEncodedContent(form));
    }

    private async Task<string> GetTokenFromPageAsync()
    {
        var page = await GetAdminDesksPageAsync();
        page.EnsureSuccessStatusCode();
        return await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");
        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }
}
