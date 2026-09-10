using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeDeskBooking.Api.Bookings;
using EmployeeDeskBooking.Api.Contracts.Admin;
using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Api.Controllers;

[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = "Admin")]
public sealed class AdminBookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AdminBookingsListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? date,
        [FromQuery] BookingStatus? status,
        CancellationToken cancellationToken)
    {
        var filters = new AdminBookingFilters
        {
            Date = date,
            Status = status,
        };

        var items = await bookingService.GetAllBookingsAsync(filters, cancellationToken);

        return Ok(new AdminBookingsListResponse
        {
            Bookings = items
                .Select(MapBooking)
                .ToList(),
        });
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AdminCancelBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await bookingService.AdminCancelBookingAsync(id, GetUserId(), cancellationToken);

        if (result.Succeeded)
        {
            return Ok(new AdminCancelBookingResponse { BookingId = id, Status = "Cancelled" });
        }

        return result.FailureReason switch
        {
            CancelBookingFailureReason.NotFound => Problem(
                detail: BookingApiMessages.CancelFailure(CancelBookingFailureReason.NotFound),
                statusCode: StatusCodes.Status404NotFound,
                title: "Booking not found"),
            _ => Problem(
                detail: BookingApiMessages.CancelFailure(CancelBookingFailureReason.NotCancellable),
                statusCode: StatusCodes.Status409Conflict,
                title: "Cannot cancel booking"),
        };
    }

    private static AdminBookingResponse MapBooking(AdminBookingItem item) =>
        new()
        {
            BookingId = item.BookingId,
            BookingDate = item.BookingDate,
            DeskNumber = item.DeskNumber,
            EmployeeEmail = item.EmployeeEmail,
            EmployeeName = item.EmployeeName,
            Status = item.Status.ToString(),
            CanCancel = item.CanCancel,
        };

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");

        return Guid.Parse(id);
    }
}
