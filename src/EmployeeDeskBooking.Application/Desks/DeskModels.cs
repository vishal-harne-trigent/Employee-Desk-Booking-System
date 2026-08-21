using EmployeeDeskBooking.Domain.Desks;

namespace EmployeeDeskBooking.Application.Desks;

public enum DeskOperationFailureReason
{
    NotFound,
    DuplicateDeskNumber,
    HasFutureBookings,
}

public sealed class DeskListItem
{
    public required Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public required DeskStatus Status { get; init; }

    public required bool CanDeactivate { get; init; }
}

public sealed class DeskOperationResult
{
    private DeskOperationResult(bool succeeded, DeskOperationFailureReason? failureReason, Guid? deskId = null)
    {
        Succeeded = succeeded;
        FailureReason = failureReason;
        DeskId = deskId;
    }

    public bool Succeeded { get; }

    public DeskOperationFailureReason? FailureReason { get; }

    public Guid? DeskId { get; }

    public static DeskOperationResult Success(Guid? deskId = null) =>
        new(true, null, deskId);

    public static DeskOperationResult Failure(DeskOperationFailureReason reason) =>
        new(false, reason);
}
