using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminUsersController(IUserAdminService userAdminService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(cancellationToken);
        if (TempData["SuccessMessage"] is string successMessage)
        {
            model.SuccessMessage = successMessage;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string email,
        string name,
        UserRole role,
        string password,
        CancellationToken cancellationToken)
    {
        var result = await userAdminService.CreateUserAsync(email, name, role, password, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminUsersViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = $"User {email.Trim()} was created.";
        return View("Index", successModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid userId,
        string email,
        string name,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var result = await userAdminService.UpdateUserAsync(userId, email, name, role, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminUsersViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = "User profile updated.";
        return View("Index", successModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userAdminService.DeactivateUserAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminUsersViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = "User deactivated.";
        return View("Index", successModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userAdminService.ActivateUserAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminUsersViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = "User activated.";
        return View("Index", successModel);
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        var model = await BuildResetPasswordViewModelAsync(userId, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        Guid userId,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken)
    {
        var userModel = await BuildResetPasswordViewModelAsync(userId, cancellationToken);
        if (userModel is null)
        {
            return NotFound();
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            userModel.ErrorMessage = AdminResetPasswordViewModel.PasswordMismatchMessage;
            userModel.NewPassword = newPassword;
            userModel.ConfirmPassword = confirmPassword;
            return View(userModel);
        }

        var result = await userAdminService.ResetPasswordAsync(userId, newPassword, cancellationToken);
        if (!result.Succeeded)
        {
            userModel.ErrorMessage = AdminUsersViewModel.ErrorMessageFor(result.FailureReason!.Value);
            userModel.NewPassword = newPassword;
            userModel.ConfirmPassword = confirmPassword;
            return View(userModel);
        }

        TempData["SuccessMessage"] = $"Password updated for {userModel.Email}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminUsersViewModel> BuildViewModelAsync(CancellationToken cancellationToken)
    {
        var users = await userAdminService.GetAllUsersAsync(cancellationToken);
        return new AdminUsersViewModel
        {
            Users = users
                .Select(u => new AdminUserRowViewModel
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CanDeactivate = u.CanDeactivate,
                    CanDemote = u.CanDemote,
                })
                .ToList(),
        };
    }

    private async Task<AdminResetPasswordViewModel?> BuildResetPasswordViewModelAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var users = await userAdminService.GetAllUsersAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.UserId == userId);
        if (user is null)
        {
            return null;
        }

        return new AdminResetPasswordViewModel
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
        };
    }
}
