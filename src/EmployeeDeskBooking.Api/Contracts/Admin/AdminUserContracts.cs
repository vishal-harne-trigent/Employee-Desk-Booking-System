namespace EmployeeDeskBooking.Api.Contracts.Admin;

public sealed class AdminUserResponse
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Role { get; init; }

    public required string Status { get; init; }

    public required bool CanDeactivate { get; init; }

    public required bool CanDemote { get; init; }
}

public sealed class AdminUsersListResponse
{
    public IReadOnlyList<AdminUserResponse> Users { get; init; } = Array.Empty<AdminUserResponse>();
}

public sealed class CreateAdminUserRequest
{
    public required string Email { get; init; }

    public required string Name { get; init; }

    public required string Role { get; init; }

    public required string Password { get; init; }
}

public sealed class UpdateAdminUserRequest
{
    public required string Email { get; init; }

    public required string Name { get; init; }

    public required string Role { get; init; }
}

public sealed class AdminUserMutationResponse
{
    public required Guid UserId { get; init; }

    public required string Status { get; init; }
}

public sealed class AdminResetPasswordRequest
{
    public required string NewPassword { get; init; }
}

public sealed class AdminResetPasswordResponse
{
    public required Guid UserId { get; init; }

    public required string Status { get; init; }
}
