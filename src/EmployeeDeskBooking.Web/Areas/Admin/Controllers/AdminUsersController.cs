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
    public async Task<IActionResult> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userAdminService.ResetPasswordAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminUsersViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var users = await userAdminService.GetAllUsersAsync(cancellationToken);
        var user = users.Single(u => u.UserId == userId);
        var model = await BuildViewModelAsync(cancellationToken);
        model.ResetPasswordForEmail = user.Email;
        model.TemporaryPassword = result.TemporaryPassword;
        model.SuccessMessage = "Password reset. Copy the temporary password now — it will not be shown again.";
        return View("Index", model);
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
}
