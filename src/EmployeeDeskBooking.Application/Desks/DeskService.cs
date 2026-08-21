using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Desks;

namespace EmployeeDeskBooking.Application.Desks;

public sealed class DeskService(
    IDeskRepository desks,
    IBookingRepository bookings,
    IOfficeClock officeClock) : IDeskService
{
    public async Task<IReadOnlyList<DeskListItem>> GetAllDesksAsync(CancellationToken cancellationToken = default)
    {
        var today = officeClock.Today;
        var allDesks = await desks.GetAllDesksAsync(cancellationToken);
        var items = new List<DeskListItem>();

        foreach (var desk in allDesks)
        {
            var hasBlockingBookings = await bookings.HasConfirmedBookingsForDeskOnOrAfterAsync(
                desk.Id,
                today,
                cancellationToken);

            items.Add(new DeskListItem
            {
                DeskId = desk.Id,
                DeskNumber = desk.DeskNumber,
                Status = desk.Status,
                CanDeactivate = desk.Status == DeskStatus.Active && !hasBlockingBookings,
            });
        }

        return items;
    }

    public async Task<DeskOperationResult> CreateDeskAsync(
        string deskNumber,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeDeskNumber(deskNumber);
        if (normalized.Length == 0)
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.DuplicateDeskNumber);
        }

        if (await desks.DeskNumberExistsAsync(normalized, cancellationToken: cancellationToken))
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.DuplicateDeskNumber);
        }

        var now = DateTimeOffset.UtcNow;
        var desk = new Desk
        {
            Id = Guid.NewGuid(),
            DeskNumber = deskNumber.Trim(),
            DeskNumberNormalized = normalized,
            Status = DeskStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await desks.AddDeskAsync(desk, cancellationToken);

        try
        {
            await desks.SaveChangesAsync(cancellationToken);
            return DeskOperationResult.Success(desk.Id);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.DuplicateDeskNumber);
        }
    }

    public async Task<DeskOperationResult> UpdateDeskNumberAsync(
        Guid deskId,
        string deskNumber,
        CancellationToken cancellationToken = default)
    {
        var desk = await desks.GetDeskByIdAsync(deskId, cancellationToken);
        if (desk is null)
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.NotFound);
        }

        var normalized = NormalizeDeskNumber(deskNumber);
        if (normalized.Length == 0)
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.DuplicateDeskNumber);
        }

        if (desk.DeskNumberNormalized == normalized)
        {
            return DeskOperationResult.Success(desk.Id);
        }

        if (await desks.DeskNumberExistsAsync(normalized, deskId, cancellationToken))
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.DuplicateDeskNumber);
        }

        desk.DeskNumber = deskNumber.Trim();
        desk.DeskNumberNormalized = normalized;
        desk.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await desks.SaveChangesAsync(cancellationToken);
            return DeskOperationResult.Success(desk.Id);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.DuplicateDeskNumber);
        }
    }

    public async Task<DeskOperationResult> DeactivateDeskAsync(
        Guid deskId,
        CancellationToken cancellationToken = default)
    {
        var desk = await desks.GetDeskByIdAsync(deskId, cancellationToken);
        if (desk is null)
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.NotFound);
        }

        if (desk.Status == DeskStatus.Inactive)
        {
            return DeskOperationResult.Success(desk.Id);
        }

        var today = officeClock.Today;
        if (await bookings.HasConfirmedBookingsForDeskOnOrAfterAsync(desk.Id, today, cancellationToken))
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.HasFutureBookings);
        }

        desk.Status = DeskStatus.Inactive;
        desk.UpdatedAt = DateTimeOffset.UtcNow;
        await desks.SaveChangesAsync(cancellationToken);
        return DeskOperationResult.Success(desk.Id);
    }

    public async Task<DeskOperationResult> ActivateDeskAsync(
        Guid deskId,
        CancellationToken cancellationToken = default)
    {
        var desk = await desks.GetDeskByIdAsync(deskId, cancellationToken);
        if (desk is null)
        {
            return DeskOperationResult.Failure(DeskOperationFailureReason.NotFound);
        }

        if (desk.Status == DeskStatus.Active)
        {
            return DeskOperationResult.Success(desk.Id);
        }

        desk.Status = DeskStatus.Active;
        desk.UpdatedAt = DateTimeOffset.UtcNow;
        await desks.SaveChangesAsync(cancellationToken);
        return DeskOperationResult.Success(desk.Id);
    }

    private static string NormalizeDeskNumber(string deskNumber) =>
        deskNumber.Trim().ToUpperInvariant();

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var message = ex.ToString();
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_Desks", StringComparison.OrdinalIgnoreCase);
    }
}
