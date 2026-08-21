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
[Authorize(Roles = "Employee")]
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
