using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeDeskBooking.Api.Bookings;
using EmployeeDeskBooking.Api.Contracts.Bookings;
using EmployeeDeskBooking.Application.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Roles = "Employee,Admin")]
public sealed class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("availability")]
    [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetAvailability([FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var availability = await bookingService.GetAvailabilityAsync(userId, date, cancellationToken);

        if (availability.HasDateError)
        {
            return Problem(
                detail: BookingApiMessages.DateError(availability.DateError!.Value),
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Invalid booking date");
        }

        return Ok(new AvailabilityResponse
        {
            Date = date,
            ExistingBookingDeskNumber = availability.ExistingBookingDeskNumber,
            Desks = availability.Desks
                .Select(d => new DeskAvailabilityResponse
                {
                    DeskId = d.DeskId,
                    DeskNumber = d.DeskNumber,
                    IsAvailable = d.IsAvailable,
                })
                .ToList(),
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await bookingService.CreateBookingAsync(
            userId,
            request.DeskId,
            request.Date,
            cancellationToken);

        if (result.Succeeded)
        {
            return CreatedAtAction(
                nameof(GetAvailability),
                new { date = request.Date },
                new CreateBookingResponse { BookingId = result.BookingId!.Value });
        }

        return MapCreateFailure(result.FailureReason!.Value);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(MyBookingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var items = await bookingService.GetMyBookingsAsync(userId, cancellationToken);

        return Ok(new MyBookingsResponse
        {
            Bookings = items
                .Select(b => new MyBookingResponse
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    DeskNumber = b.DeskNumber,
                    Status = b.Status.ToString(),
                    CanCancel = b.CanCancel,
                })
                .ToList(),
        });
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelBooking(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await bookingService.CancelBookingAsync(userId, id, userId, cancellationToken);

        if (result.Succeeded)
        {
            return Ok(new CancelBookingResponse { BookingId = id, Status = "Cancelled" });
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

    private IActionResult MapCreateFailure(CreateBookingFailureReason reason) =>
        reason switch
        {
            CreateBookingFailureReason.DeskNotFound => Problem(
                detail: "Desk was not found.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Desk not found"),
            CreateBookingFailureReason.InvalidDate or CreateBookingFailureReason.DeskInactive => Problem(
                detail: BookingApiMessages.CreateFailure(reason),
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Booking rejected"),
            CreateBookingFailureReason.UserAlreadyBooked
                or CreateBookingFailureReason.DeskAlreadyBooked
                or CreateBookingFailureReason.ConcurrencyConflict => Problem(
                    detail: BookingApiMessages.CreateFailure(reason),
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Booking conflict"),
            _ => Problem(
                detail: BookingApiMessages.CreateFailure(reason),
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Booking rejected"),
        };

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");

        return Guid.Parse(id);
    }
}
