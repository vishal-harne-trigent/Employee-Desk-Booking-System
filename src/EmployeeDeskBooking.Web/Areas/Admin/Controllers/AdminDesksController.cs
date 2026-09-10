using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminDesksController(IDeskService deskService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string deskNumber, string? location, CancellationToken cancellationToken)
    {
        var result = await deskService.CreateDeskAsync(deskNumber, location, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminDesksViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = $"Desk {deskNumber.Trim()} was added.";
        return View("Index", successModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid deskId,
        string deskNumber,
        string? location,
        CancellationToken cancellationToken)
    {
        var result = await deskService.UpdateDeskAsync(deskId, deskNumber, location, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminDesksViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = "Desk updated.";
        return View("Index", successModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid deskId, CancellationToken cancellationToken)
    {
        var result = await deskService.DeactivateDeskAsync(deskId, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminDesksViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = "Desk deactivated.";
        return View("Index", successModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid deskId, CancellationToken cancellationToken)
    {
        var result = await deskService.ActivateDeskAsync(deskId, cancellationToken);
        if (!result.Succeeded)
        {
            var errorModel = await BuildViewModelAsync(cancellationToken);
            errorModel.ErrorMessage = AdminDesksViewModel.ErrorMessageFor(result.FailureReason!.Value);
            return View("Index", errorModel);
        }

        var successModel = await BuildViewModelAsync(cancellationToken);
        successModel.SuccessMessage = "Desk activated.";
        return View("Index", successModel);
    }

    private async Task<AdminDesksViewModel> BuildViewModelAsync(CancellationToken cancellationToken)
    {
        var desks = await deskService.GetAllDesksAsync(cancellationToken);
        return new AdminDesksViewModel
        {
            Desks = desks
                .Select(d => new AdminDeskRowViewModel
                {
                    DeskId = d.DeskId,
                    DeskNumber = d.DeskNumber,
                    Status = d.Status,
                    CanDeactivate = d.CanDeactivate,
                    Location = d.Location,
                })
                .ToList(),
        };
    }
}
