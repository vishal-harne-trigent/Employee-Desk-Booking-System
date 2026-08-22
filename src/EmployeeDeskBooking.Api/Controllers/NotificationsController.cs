using System.Security.Claims;
using System.Text.Json;
using EmployeeDeskBooking.Api.Contracts.Notifications;
using EmployeeDeskBooking.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Employee,Admin")]
public sealed class NotificationsController(INotificationPreferenceService preferences) : ControllerBase
{
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var state = await preferences.GetPreferencesAsync(GetUserId(), cancellationToken);
        return Ok(new NotificationPreferencesResponse
        {
            PushOptIn = state.PushOptIn,
            HasSubscription = state.HasSubscription,
        });
    }

    [HttpPatch("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (!request.PushOptIn)
        {
            await preferences.OptOutAsync(userId, cancellationToken);
        }
        else
        {
            return Problem(
                detail: "Use POST /api/notifications/push-subscription to opt in with a browser subscription.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid preference update");
        }

        var state = await preferences.GetPreferencesAsync(userId, cancellationToken);
        return Ok(new NotificationPreferencesResponse
        {
            PushOptIn = state.PushOptIn,
            HasSubscription = state.HasSubscription,
        });
    }

    [HttpPost("push-subscription")]
    [ProducesResponseType(typeof(NotificationPreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SavePushSubscription(
        [FromBody] SavePushSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var subscriptionJson = JsonSerializer.Serialize(new
        {
            endpoint = request.Subscription.Endpoint,
            keys = new
            {
                p256dh = request.Subscription.Keys.P256dh,
                auth = request.Subscription.Keys.Auth,
            },
        });

        try
        {
            await preferences.OptInAsync(GetUserId(), subscriptionJson, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid push subscription");
        }

        var state = await preferences.GetPreferencesAsync(GetUserId(), cancellationToken);
        return Ok(new NotificationPreferencesResponse
        {
            PushOptIn = state.PushOptIn,
            HasSubscription = state.HasSubscription,
        });
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user id was not found.");
        return Guid.Parse(id);
    }
}
