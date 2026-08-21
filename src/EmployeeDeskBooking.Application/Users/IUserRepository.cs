using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
