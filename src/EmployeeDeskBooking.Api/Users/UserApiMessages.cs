using EmployeeDeskBooking.Application.Users;

namespace EmployeeDeskBooking.Api.Users;

public static class UserApiMessages
{
    public static string Failure(UserAdminFailureReason reason) =>
        reason switch
        {
            UserAdminFailureReason.NotFound => "User was not found.",
            UserAdminFailureReason.DuplicateEmail =>
                "Email is already in use. Choose a unique email address.",
            UserAdminFailureReason.LastAdminProtected =>
                "This action would remove the last active Admin. Assign another Admin first.",
            UserAdminFailureReason.InvalidPassword => "Password is required.",
            _ => "Unable to complete the user operation.",
        };
}
