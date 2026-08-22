namespace EmployeeDeskBooking.Web.Areas.Admin.Models;

public sealed class AdminResetPasswordViewModel
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? NewPassword { get; set; }

    public string? ConfirmPassword { get; set; }

    public string? ErrorMessage { get; set; }

    public static string PasswordMismatchMessage => "Passwords do not match. Enter the same value in both fields.";
}
