using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDeskBooking.Infrastructure.Desks;

public sealed class EfDeskRepository(AppDbContext dbContext) : IDeskRepository
{
    public async Task<IReadOnlyList<Desk>> GetAllDesksAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Desks
            .AsNoTracking()
            .OrderBy(d => d.DeskNumberNormalized)
            .ToListAsync(cancellationToken);

    public Task<Desk?> GetDeskByIdAsync(Guid deskId, CancellationToken cancellationToken = default) =>
        dbContext.Desks.FirstOrDefaultAsync(d => d.Id == deskId, cancellationToken);

    public Task<bool> DeskNumberExistsAsync(
        string deskNumberNormalized,
        Guid? excludeDeskId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Desks.AsNoTracking()
            .Where(d => d.DeskNumberNormalized == deskNumberNormalized);

        if (excludeDeskId.HasValue)
        {
            query = query.Where(d => d.Id != excludeDeskId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task AddDeskAsync(Desk desk, CancellationToken cancellationToken = default)
    {
        dbContext.Desks.Add(desk);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
