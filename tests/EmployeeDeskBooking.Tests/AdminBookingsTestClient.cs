using AngleSharp;
using AngleSharp.Html.Dom;
using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Tests;

public sealed class AdminBookingsTestClient(CustomWebApplicationFactory factory)
{
    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public LoginTestClient Login { get; } = factory.CreateLoginTestClient();

    public async Task LoginAsAdminAsync()
    {
        var response = await Login.LoginAsync("admin@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(302, (int)response.StatusCode);
        Assert.StartsWith("/Admin/AdminBookings", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    public Task<HttpResponseMessage> GetAdminBookingsPageAsync() =>
        Login.Client.GetAsync("/Admin/AdminBookings");

    public async Task<HttpResponseMessage> ApplyFiltersAsync(DateOnly? date, BookingStatus? status)
    {
        var page = await GetAdminBookingsPageAsync();
        page.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
        };

        if (date.HasValue)
        {
            form["filterDate"] = date.Value.ToString("yyyy-MM-dd");
        }

        if (status.HasValue)
        {
            form["filterStatus"] = ((int)status.Value).ToString();
        }

        return await Login.Client.PostAsync("/Admin/AdminBookings/ApplyFilters", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> CancelBookingAsync(Guid bookingId, DateOnly? filterDate, BookingStatus? filterStatus)
    {
        var page = await GetAdminBookingsPageAsync();
        page.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());

        var form = new Dictionary<string, string>
        {
            ["bookingId"] = bookingId.ToString(),
            ["__RequestVerificationToken"] = token,
        };

        if (filterDate.HasValue)
        {
            form["filterDate"] = filterDate.Value.ToString("yyyy-MM-dd");
        }

        if (filterStatus.HasValue)
        {
            form["filterStatus"] = ((int)filterStatus.Value).ToString();
        }

        return await Login.Client.PostAsync("/Admin/AdminBookings/Cancel", new FormUrlEncodedContent(form));
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");
        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }
}
