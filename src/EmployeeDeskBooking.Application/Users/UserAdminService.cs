using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Application.Users;

public sealed class UserAdminService(IUserRepository users, IPasswordVerifier passwordVerifier) : IUserAdminService
{
    public async Task<IReadOnlyList<AdminUserListItem>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var activeAdminCount = await users.CountActiveAdminsAsync(cancellationToken);
        var allUsers = await users.GetAllUsersAsync(cancellationToken);

        return allUsers
            .Select(user => MapListItem(user, activeAdminCount))
            .ToList();
    }

    public async Task<UserAdminResult> CreateUserAsync(
        string email,
        string name,
        UserRole role,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return UserAdminResult.Failure(UserAdminFailureReason.InvalidPassword);
        }

        if (await users.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            return UserAdminResult.Failure(UserAdminFailureReason.DuplicateEmail);
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            Name = name.Trim(),
            Role = role,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.PasswordHash = passwordVerifier.HashPassword(user, password);

        await users.AddAsync(user, cancellationToken);

        try
        {
            await users.SaveChangesAsync(cancellationToken);
            return UserAdminResult.Success(user.Id);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return UserAdminResult.Failure(UserAdminFailureReason.DuplicateEmail);
        }
    }

    public async Task<UserAdminResult> UpdateUserAsync(
        Guid userId,
        string email,
        string name,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return UserAdminResult.Failure(UserAdminFailureReason.NotFound);
        }

        if (await users.EmailExistsAsync(email, userId, cancellationToken))
        {
            return UserAdminResult.Failure(UserAdminFailureReason.DuplicateEmail);
        }

        if (user.IsActive
            && user.Role == UserRole.Admin
            && role == UserRole.Employee
            && await users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return UserAdminResult.Failure(UserAdminFailureReason.LastAdminProtected);
        }

        user.Email = email.Trim();
        user.Name = name.Trim();
        user.Role = role;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await users.SaveChangesAsync(cancellationToken);
            return UserAdminResult.Success(user.Id);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return UserAdminResult.Failure(UserAdminFailureReason.DuplicateEmail);
        }
    }

    public async Task<UserAdminResult> DeactivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return UserAdminResult.Failure(UserAdminFailureReason.NotFound);
        }

        if (!user.IsActive)
        {
            return UserAdminResult.Success(user.Id);
        }

        if (user.Role == UserRole.Admin && await users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return UserAdminResult.Failure(UserAdminFailureReason.LastAdminProtected);
        }

        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.SaveChangesAsync(cancellationToken);
        return UserAdminResult.Success(user.Id);
    }

    public async Task<UserAdminResult> ActivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return UserAdminResult.Failure(UserAdminFailureReason.NotFound);
        }

        if (user.IsActive)
        {
            return UserAdminResult.Success(user.Id);
        }

        user.IsActive = true;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.SaveChangesAsync(cancellationToken);
        return UserAdminResult.Success(user.Id);
    }

    public async Task<UserAdminResult> ResetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return UserAdminResult.Failure(UserAdminFailureReason.InvalidPassword);
        }

        var user = await users.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return UserAdminResult.Failure(UserAdminFailureReason.NotFound);
        }

        user.PasswordHash = passwordVerifier.HashPassword(user, newPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.SaveChangesAsync(cancellationToken);
        return UserAdminResult.Success(user.Id);
    }

    private static AdminUserListItem MapListItem(User user, int activeAdminCount)
    {
        var isLastActiveAdmin = user.IsActive && user.Role == UserRole.Admin && activeAdminCount <= 1;
        return new AdminUserListItem
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CanDeactivate = user.IsActive && !isLastActiveAdmin,
            CanDemote = user.IsActive && user.Role == UserRole.Admin && activeAdminCount > 1,
        };
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var message = ex.ToString();
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_Users", StringComparison.OrdinalIgnoreCase);
    }
}
