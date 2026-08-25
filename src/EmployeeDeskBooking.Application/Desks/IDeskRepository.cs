using EmployeeDeskBooking.Domain.Desks;

namespace EmployeeDeskBooking.Application.Desks;

public interface IDeskRepository
{
    Task<IReadOnlyList<Desk>> GetAllDesksAsync(CancellationToken cancellationToken = default);

    Task<Desk?> GetDeskByIdAsync(Guid deskId, CancellationToken cancellationToken = default);

    Task<bool> DeskNumberExistsAsync(
        string deskNumberNormalized,
        Guid? excludeDeskId = null,
        CancellationToken cancellationToken = default);

    Task AddDeskAsync(Desk desk, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
