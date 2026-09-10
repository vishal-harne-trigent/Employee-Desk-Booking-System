using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class BookingCompletionTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Past Confirmed booking becomes Completed (US-009/AC-01)")]
    public async Task Past_confirmed_becomes_completed_US_009_AC_01()
    {
        await ResetAsync();
        Guid bookingId;
        using (var scope = factory.Services.CreateScope())
        {
            var seedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = seedDb.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(seedDb, employee.Id, "A-01", Yesterday);
            bookingId = seedDb.Bookings.Single().Id;
        }

        await RunCompletionJobAsync();

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = db.Bookings.Single(b => b.Id == bookingId);
        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.NotNull(booking.CompletedAt);
    }

    [Fact(DisplayName = "Cancelled bookings remain unchanged (US-009/AC-02)")]
    public async Task Cancelled_bookings_unchanged_US_009_AC_02()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var seedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = seedDb.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(seedDb, employee.Id, "A-01", Yesterday, BookingStatus.Cancelled);
        }

        await RunCompletionJobAsync();

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(BookingStatus.Cancelled, db.Bookings.Single().Status);
        Assert.Null(db.Bookings.Single().CompletedAt);
    }

    [Fact(DisplayName = "Todays Confirmed booking stays Confirmed (US-009/AC-03)")]
    public async Task Todays_confirmed_stays_confirmed_US_009_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var seedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = seedDb.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(seedDb, employee.Id, "A-01", Today);
        }

        await RunCompletionJobAsync();

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(BookingStatus.Confirmed, db.Bookings.Single().Status);
        Assert.Null(db.Bookings.Single().CompletedAt);
    }

    private async Task RunCompletionJobAsync()
    {
        using var scope = factory.Services.CreateScope();
        var completionService = scope.ServiceProvider.GetRequiredService<IBookingCompletionService>();
        await completionService.CompletePastBookingsAsync();
    }
}
