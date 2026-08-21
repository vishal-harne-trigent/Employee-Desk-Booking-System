using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public sealed class MyBookingsTestClient(CustomWebApplicationFactory factory)
{
    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public LoginTestClient Login { get; } = factory.CreateLoginTestClient();

    public async Task LoginAsEmployeeAsync()
    {
        var response = await Login.LoginAsync("employee@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(302, (int)response.StatusCode);
    }

    public async Task<HttpResponseMessage> GetMyBookingsPageAsync()
    {
        return await Login.Client.GetAsync("/MyBookings/Index");
    }

    public async Task<HttpResponseMessage> CancelBookingAsync(Guid bookingId)
    {
        var page = await GetMyBookingsPageAsync();
        page.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["bookingId"] = bookingId.ToString(),
            ["__RequestVerificationToken"] = token,
        });

        return await Login.Client.PostAsync("/MyBookings/Cancel", form);
    }

    public Guid GetBookingIdForEmployee(string deskNumber, DateOnly date)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmployeeDeskBooking.Infrastructure.Data.AppDbContext>();
        var employee = db.Users.Single(u => u.Email == "employee@test.com");
        var desk = db.Desks.Single(d => d.DeskNumber == deskNumber);
        return db.Bookings.Single(b =>
            b.UserId == employee.Id && b.DeskId == desk.Id && b.BookingDate == date).Id;
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");
        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }
}
