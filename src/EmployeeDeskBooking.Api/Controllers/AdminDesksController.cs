using EmployeeDeskBooking.Api.Contracts.Admin;
using EmployeeDeskBooking.Api.Desks;
using EmployeeDeskBooking.Application.Desks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Api.Controllers;

[ApiController]
[Route("api/admin/desks")]
[Authorize(Roles = "Admin")]
public sealed class AdminDesksController(IDeskService deskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AdminDesksListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var desks = await deskService.GetAllDesksAsync(cancellationToken);
        return Ok(new AdminDesksListResponse
        {
            Desks = desks.Select(MapDesk).ToList(),
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminDeskMutationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdminDeskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await deskService.CreateDeskAsync(request.DeskNumber, request.Location, cancellationToken);
        if (result.Succeeded)
        {
            return CreatedAtAction(
                nameof(GetAll),
                new AdminDeskMutationResponse
                {
                    DeskId = result.DeskId!.Value,
                    Status = "Active",
                });
        }

        return ConflictProblem(result.FailureReason!.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminDeskMutationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAdminDeskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await deskService.UpdateDeskAsync(id, request.DeskNumber, request.Location, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(new AdminDeskMutationResponse { DeskId = id, Status = "Updated" });
        }

        return result.FailureReason switch
        {
            DeskOperationFailureReason.NotFound => NotFoundProblem(result.FailureReason.Value),
            _ => ConflictProblem(result.FailureReason!.Value),
        };
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(AdminDeskMutationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await deskService.DeactivateDeskAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(new AdminDeskMutationResponse { DeskId = id, Status = "Inactive" });
        }

        return result.FailureReason switch
        {
            DeskOperationFailureReason.NotFound => NotFoundProblem(result.FailureReason.Value),
            _ => ConflictProblem(result.FailureReason!.Value),
        };
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(AdminDeskMutationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await deskService.ActivateDeskAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(new AdminDeskMutationResponse { DeskId = id, Status = "Active" });
        }

        return NotFoundProblem(result.FailureReason!.Value);
    }

    private static AdminDeskResponse MapDesk(DeskListItem item) =>
        new()
        {
            DeskId = item.DeskId,
            DeskNumber = item.DeskNumber,
            Location = item.Location,
            Status = item.Status.ToString(),
            CanDeactivate = item.CanDeactivate,
        };

    private IActionResult ConflictProblem(DeskOperationFailureReason reason) =>
        Problem(
            detail: DeskApiMessages.Failure(reason),
            statusCode: StatusCodes.Status409Conflict,
            title: "Desk operation failed");

    private IActionResult NotFoundProblem(DeskOperationFailureReason reason) =>
        Problem(
            detail: DeskApiMessages.Failure(reason),
            statusCode: StatusCodes.Status404NotFound,
            title: "Desk not found");
}
