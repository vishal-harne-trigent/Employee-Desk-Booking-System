using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Web.Helpers;
using EmployeeDeskBooking.Web.Models;
using EmployeeDeskBooking.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeDeskBooking.Web.Controllers;

[Authorize(Roles = "Employee")]
public class DesksController(
    IBookingService bookingService,
    BookPageModelFactory bookPageModelFactory) : Controller
{
    [HttpGet]
    public Task<IActionResult> Availability(DateOnly? date, CancellationToken cancellationToken) =>
        RenderAvailabilityAsync(date, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> Book(Guid deskId, DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await bookingService.CreateBookingAsync(userId, deskId, date, cancellationToken);

        if (!result.Succeeded)
        {
            var errorModel = await bookPageModelFactory.BuildAsync(userId, date, true, cancellationToken);
            errorModel.BookingError = BookIndexViewModel.BookingErrorMessage(result.FailureReason!.Value);
            return View("~/Views/Book/Index.cshtml", errorModel);
        }

        var successModel = await bookPageModelFactory.BuildAsync(userId, date, true, cancellationToken);
        if (successModel.ExistingBooking is null)
        {
            var bookedDeskNumber = successModel.Desks.FirstOrDefault(d => d.DeskId == deskId)?.DeskNumber;
            if (bookedDeskNumber is not null)
            {
                successModel.ExistingBooking = new ExistingBookingRowViewModel
                {
                    DeskNumber = bookedDeskNumber,
                    Location = DeskLocationHelper.FormatLocation(bookedDeskNumber),
                    BookingDate = date,
                };
            }
        }

        var confirmedDeskNumber = successModel.ExistingBooking?.DeskNumber
            ?? successModel.Desks.FirstOrDefault(d => d.DeskId == deskId)?.DeskNumber
            ?? "your desk";
        successModel.SuccessMessage = BookIndexViewModel.BookingConfirmedMessage(confirmedDeskNumber, date);
        if (!result.EmailNotificationSent)
        {
            successModel.EmailWarning =
                "We could not send a confirmation email. Add Smtp:Username and Smtp:Password (see appsettings.Development.local.json.example), then restart the app.";
        }
        return View("~/Views/Book/Index.cshtml", successModel);
    }

    private async Task<IActionResult> RenderAvailabilityAsync(DateOnly? date, CancellationToken cancellationToken)
    {
        var model = await bookPageModelFactory.BuildAsync(
            GetUserId(),
            date,
            availabilityRequested: true,
            cancellationToken);
        return View("~/Views/Book/Index.cshtml", model);
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
        return Guid.Parse(id);
    }
}
