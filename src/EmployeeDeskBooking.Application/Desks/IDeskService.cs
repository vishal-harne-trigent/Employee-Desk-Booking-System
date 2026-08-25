namespace EmployeeDeskBooking.Application.Desks;

public interface IDeskService
{
    Task<IReadOnlyList<DeskListItem>> GetAllDesksAsync(CancellationToken cancellationToken = default);

    Task<DeskOperationResult> CreateDeskAsync(
        string deskNumber,
        string? location = null,
        CancellationToken cancellationToken = default);

    Task<DeskOperationResult> UpdateDeskAsync(
        Guid deskId,
        string deskNumber,
        string? location = null,
        CancellationToken cancellationToken = default);

    Task<DeskOperationResult> DeactivateDeskAsync(Guid deskId, CancellationToken cancellationToken = default);

    Task<DeskOperationResult> ActivateDeskAsync(Guid deskId, CancellationToken cancellationToken = default);
}
