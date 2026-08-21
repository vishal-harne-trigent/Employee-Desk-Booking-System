using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeDeskBooking.Web.Controllers;

[Authorize(Roles = "Employee")]
public class MyBookingsController(IBookingService bookingService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(GetUserId(), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid bookingId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await bookingService.CancelBookingAsync(userId, bookingId, userId, cancellationToken);

        if (!result.Succeeded)
        {
            var model = await BuildViewModelAsync(userId, cancellationToken);
            model.ErrorMessage = MyBookingsViewModel.CancelErrorMessage(result.FailureReason!.Value);
            return View("Index", model);
        }

        var success = await BuildViewModelAsync(userId, cancellationToken);
        success.SuccessMessage = "Your booking has been cancelled.";
        return View("Index", success);
    }

    private async Task<MyBookingsViewModel> BuildViewModelAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingService.GetMyBookingsAsync(userId, cancellationToken);

        return new MyBookingsViewModel
        {
            Bookings = bookings
                .Select(b => new MyBookingRowViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    DeskNumber = b.DeskNumber,
                    Status = b.Status,
                    CanCancel = b.CanCancel,
                })
                .ToList(),
        };
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
        return Guid.Parse(id);
    }
}
