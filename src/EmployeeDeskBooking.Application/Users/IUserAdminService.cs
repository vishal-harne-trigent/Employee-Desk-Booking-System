using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Application.Users;

public interface IUserAdminService
{
    Task<IReadOnlyList<AdminUserListItem>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    Task<UserAdminResult> CreateUserAsync(
        string email,
        string name,
        UserRole role,
        string password,
        CancellationToken cancellationToken = default);

    Task<UserAdminResult> UpdateUserAsync(
        Guid userId,
        string email,
        string name,
        UserRole role,
        CancellationToken cancellationToken = default);

    Task<UserAdminResult> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserAdminResult> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserAdminResult> ResetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default);
}
