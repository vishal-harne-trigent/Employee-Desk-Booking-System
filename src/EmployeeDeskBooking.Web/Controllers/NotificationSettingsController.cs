using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Infrastructure.Notifications;
using EmployeeDeskBooking.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EmployeeDeskBooking.Web.Controllers;

[Authorize(Roles = "Employee,Admin")]
[Route("Settings/Notifications")]
public sealed class NotificationSettingsController(
    INotificationPreferenceService preferences,
    IOptions<VapidOptions> vapidOptions) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildViewModelAsync(GetUserId(), cancellationToken);
        return View("Index", model);
    }

    [HttpPost("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(CancellationToken cancellationToken)
    {
        await preferences.OptOutAsync(GetUserId(), cancellationToken);
        var model = await BuildViewModelAsync(GetUserId(), cancellationToken);
        model = model with { SuccessMessage = "Browser push notifications are disabled." };
        return View("Index", model);
    }

    [HttpPost("SaveSubscription")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSubscription(
        string subscriptionJson,
        CancellationToken cancellationToken)
    {
        try
        {
            await preferences.OptInAsync(GetUserId(), subscriptionJson, cancellationToken);
            var success = await BuildViewModelAsync(GetUserId(), cancellationToken);
            success = success with { SuccessMessage = "Browser push notifications are enabled." };
            return View("Index", success);
        }
        catch (ArgumentException)
        {
            var error = await BuildViewModelAsync(GetUserId(), cancellationToken);
            error = error with { ErrorMessage = "Could not save push subscription. Email alerts are still active." };
            return View("Index", error);
        }
    }

    private async Task<NotificationSettingsViewModel> BuildViewModelAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var state = await preferences.GetPreferencesAsync(userId, cancellationToken);
        return new NotificationSettingsViewModel
        {
            PushOptIn = state.PushOptIn,
            HasSubscription = state.HasSubscription,
            VapidPublicKey = vapidOptions.Value.PublicKey,
        };
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
        return Guid.Parse(id);
    }
}
