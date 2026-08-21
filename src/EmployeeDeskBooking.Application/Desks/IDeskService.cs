namespace EmployeeDeskBooking.Application.Desks;

public interface IDeskService
{
    Task<IReadOnlyList<DeskListItem>> GetAllDesksAsync(CancellationToken cancellationToken = default);

    Task<DeskOperationResult> CreateDeskAsync(string deskNumber, CancellationToken cancellationToken = default);

    Task<DeskOperationResult> UpdateDeskNumberAsync(
        Guid deskId,
        string deskNumber,
        CancellationToken cancellationToken = default);

    Task<DeskOperationResult> DeactivateDeskAsync(Guid deskId, CancellationToken cancellationToken = default);

    Task<DeskOperationResult> ActivateDeskAsync(Guid deskId, CancellationToken cancellationToken = default);
}
