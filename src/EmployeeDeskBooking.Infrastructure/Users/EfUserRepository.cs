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
