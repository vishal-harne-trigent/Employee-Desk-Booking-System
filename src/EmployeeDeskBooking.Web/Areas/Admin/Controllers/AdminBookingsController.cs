using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Web.Areas.Admin.Models;
using EmployeeDeskBooking.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeDeskBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminBookingsController(
    IBookingService bookingService,
    IConfiguration configuration) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? filterDate, BookingStatus? filterStatus, CancellationToken cancellationToken)
    {
        var filtersApplied = filterDate.HasValue || filterStatus.HasValue;
        var model = await BuildViewModelAsync(filterDate, filterStatus, filtersApplied, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyFilters(DateOnly? filterDate, BookingStatus? filterStatus, CancellationToken cancellationToken)
    {
        var filtersApplied = filterDate.HasValue || filterStatus.HasValue;
        var model = await BuildViewModelAsync(filterDate, filterStatus, filtersApplied, cancellationToken);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        Guid bookingId,
        DateOnly? filterDate,
        BookingStatus? filterStatus,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var result = await bookingService.AdminCancelBookingAsync(bookingId, adminId, cancellationToken);

        var filtersApplied = filterDate.HasValue || filterStatus.HasValue;

        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(filterDate, filterStatus, filtersApplied, cancellationToken);
            errorModel.ErrorMessage = AdminBookingsViewModel.CancelErrorMessage(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(filterDate, filterStatus, filtersApplied, cancellationToken);
        successModel.SuccessMessage = "Booking cancelled on behalf of the employee.";
        return View("Index", successModel);
    }

    private async Task<AdminBookingsViewModel> BuildViewModelAsync(
        DateOnly? filterDate,
        BookingStatus? filterStatus,
        bool filtersApplied,
        CancellationToken cancellationToken)
    {
        AdminBookingFilters? filters = filtersApplied
            ? new AdminBookingFilters { Date = filterDate, Status = filterStatus }
            : null;

        var bookings = await bookingService.GetAllBookingsAsync(filters, cancellationToken);
        var officeTimeZone = AdminBookingDisplayHelper.GetOfficeTimeZone(configuration);

        return new AdminBookingsViewModel
        {
            FilterDate = filterDate,
            FilterStatus = filterStatus,
            FiltersApplied = filtersApplied,
            Bookings = bookings
                .Select(b => new AdminBookingRowViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    DeskNumber = b.DeskNumber,
                    EmployeeEmail = b.EmployeeEmail,
                    EmployeeName = b.EmployeeName,
                    Status = b.Status,
                    CanCancel = b.CanCancel,
                    OfficeDateDisplay = AdminBookingDisplayHelper.FormatOfficeDate(b.BookingDate),
                    CreatedDisplay = AdminBookingDisplayHelper.FormatTimestamp(b.CreatedAt, officeTimeZone),
                    CancelledOnDisplay = b.CancelledAt.HasValue
                        ? AdminBookingDisplayHelper.FormatTimestamp(b.CancelledAt.Value, officeTimeZone)
                        : "—",
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
