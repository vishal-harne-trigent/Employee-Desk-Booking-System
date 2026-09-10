using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Web.Areas.Admin.Models;

public class AdminUsersViewModel
{
    public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = Array.Empty<AdminUserRowViewModel>();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public static string ErrorMessageFor(UserAdminFailureReason reason) =>
        reason switch
        {
            UserAdminFailureReason.NotFound => "That user was not found.",
            UserAdminFailureReason.DuplicateEmail =>
                "Email is already in use. Choose a unique email address.",
            UserAdminFailureReason.LastAdminProtected =>
                "This action would remove the last active Admin. Assign another Admin first.",
            UserAdminFailureReason.InvalidPassword => "Password is required.",
            _ => "Unable to complete the user operation. Please try again.",
        };
}

public sealed class AdminUserRowViewModel
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required UserRole Role { get; init; }

    public required bool IsActive { get; init; }

    public required bool CanDeactivate { get; init; }

    public required bool CanDemote { get; init; }

    public string RoleLabel => Role.ToString();

    public string StatusLabel => IsActive ? "Active" : "Inactive";
}
