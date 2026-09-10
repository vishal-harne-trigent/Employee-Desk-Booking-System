using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Application.Users;

public enum UserAdminFailureReason
{
    NotFound,
    DuplicateEmail,
    LastAdminProtected,
    InvalidPassword,
}

public sealed class AdminUserListItem
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required UserRole Role { get; init; }

    public required bool IsActive { get; init; }

    public required bool CanDeactivate { get; init; }

    public required bool CanDemote { get; init; }
}

public sealed class UserAdminResult
{
    private UserAdminResult(bool succeeded, UserAdminFailureReason? failureReason, Guid? userId = null, string? temporaryPassword = null)
    {
        Succeeded = succeeded;
        FailureReason = failureReason;
        UserId = userId;
        TemporaryPassword = temporaryPassword;
    }

    public bool Succeeded { get; }

    public UserAdminFailureReason? FailureReason { get; }

    public Guid? UserId { get; }

    public string? TemporaryPassword { get; }

    public static UserAdminResult Success(Guid? userId = null, string? temporaryPassword = null) =>
        new(true, null, userId, temporaryPassword);

    public static UserAdminResult Failure(UserAdminFailureReason reason) =>
        new(false, reason);
}
