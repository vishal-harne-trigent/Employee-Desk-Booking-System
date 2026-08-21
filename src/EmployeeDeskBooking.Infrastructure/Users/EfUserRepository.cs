using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDeskBooking.Infrastructure.Users;

public sealed class EfUserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.EmailNormalized == normalized, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeEmail(email);
        var query = dbContext.Users.AsNoTracking()
            .Where(user => user.EmailNormalized == normalized);

        if (excludeUserId.HasValue)
        {
            query = query.Where(user => user.Id != excludeUserId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users.CountAsync(
            user => user.Role == UserRole.Admin && user.IsActive,
            cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        user.EmailNormalized = NormalizeEmail(user.Email);
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    internal static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
