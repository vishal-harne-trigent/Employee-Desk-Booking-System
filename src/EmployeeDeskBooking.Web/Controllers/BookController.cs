using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeDeskBooking.Web.Controllers;

[Authorize(Roles = "Employee")]
public class BookController(IBookingService bookingService, IOfficeClock officeClock) : Controller
{
    [HttpGet]
    public IActionResult Index(DateOnly? date)
    {
        var model = new BookIndexViewModel
        {
            SelectedDate = date ?? officeClock.Today,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckAvailability(DateOnly selectedDate, CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(GetUserId(), selectedDate, availabilityRequested: true, cancellationToken);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookDesk(
        Guid deskId,
        DateOnly selectedDate,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await bookingService.CreateBookingAsync(userId, deskId, selectedDate, cancellationToken);

        if (!result.Succeeded)
        {
            var model = await BuildViewModelAsync(userId, selectedDate, availabilityRequested: true, cancellationToken);
            model.BookingError = BookIndexViewModel.BookingErrorMessage(result.FailureReason!.Value);
            return View("Index", model);
        }

        var successModel = await BuildViewModelAsync(userId, selectedDate, availabilityRequested: true, cancellationToken);
        successModel.SuccessMessage = "Your desk booking is confirmed.";
        return View("Index", successModel);
    }

    private async Task<BookIndexViewModel> BuildViewModelAsync(
        Guid userId,
        DateOnly selectedDate,
        bool availabilityRequested,
        CancellationToken cancellationToken)
    {
        var model = new BookIndexViewModel
        {
            SelectedDate = selectedDate,
            AvailabilityRequested = availabilityRequested,
        };

        if (!availabilityRequested)
        {
            return model;
        }

        var availability = await bookingService.GetAvailabilityAsync(userId, selectedDate, cancellationToken);
        if (availability.HasDateError)
        {
            model.DateError = BookIndexViewModel.DateErrorMessage(availability.DateError!.Value);
            return model;
        }

        model.ExistingBookingDeskNumber = availability.ExistingBookingDeskNumber;
        model.Desks = availability.Desks
            .Select(d => new DeskRowViewModel
            {
                DeskId = d.DeskId,
                DeskNumber = d.DeskNumber,
                IsAvailable = d.IsAvailable,
            })
            .ToList();

        return model;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
        return Guid.Parse(id);
    }
}
