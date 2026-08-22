using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Web.Helpers;
using EmployeeDeskBooking.Web.Models;

namespace EmployeeDeskBooking.Web.Services;

public sealed class BookPageModelFactory(IBookingService bookingService, IOfficeClock officeClock)
{
    public async Task<BookIndexViewModel> BuildAsync(
        Guid userId,
        DateOnly? selectedDate,
        bool availabilityRequested,
        CancellationToken cancellationToken = default)
    {
        var date = selectedDate ?? officeClock.Today;
        var model = new BookIndexViewModel
        {
            SelectedDate = date,
            AvailabilityRequested = availabilityRequested,
            MinBookingDate = officeClock.Today,
            MaxBookingDate = officeClock.Today.AddDays(30),
        };

        if (!availabilityRequested)
        {
            return model;
        }

        var availability = await bookingService.GetAvailabilityAsync(userId, date, cancellationToken);
        if (availability.HasDateError)
        {
            model.DateError = BookIndexViewModel.DateErrorMessage(availability.DateError!.Value);
            return model;
        }

        model.ExistingBooking = availability.ExistingBookingDeskNumber is null
            ? null
            : new ExistingBookingRowViewModel
            {
                DeskNumber = availability.ExistingBookingDeskNumber,
                Location = DeskLocationHelper.FormatLocation(availability.ExistingBookingDeskNumber),
                BookingDate = date,
            };
        model.Desks = availability.Desks
            .Select(d => new DeskRowViewModel
            {
                DeskId = d.DeskId,
                DeskNumber = d.DeskNumber,
                Location = DeskLocationHelper.FormatLocation(d.DeskNumber),
                IsAvailable = d.IsAvailable,
            })
            .ToList();

        return model;
    }
}
