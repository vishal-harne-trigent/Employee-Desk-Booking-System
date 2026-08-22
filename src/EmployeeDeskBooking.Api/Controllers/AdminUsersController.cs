using EmployeeDeskBooking.Api.Contracts.Admin;
using EmployeeDeskBooking.Api.Users;
using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController(IUserAdminService userAdminService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AdminUsersListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await userAdminService.GetAllUsersAsync(cancellationToken);
        return Ok(new AdminUsersListResponse
        {
            Users = users.Select(MapUser).ToList(),
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminUserMutationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseRole(request.Role, out var role))
        {
            return BadRequestProblem("Role must be Employee or Admin.");
        }

        var result = await userAdminService.CreateUserAsync(
            request.Email,
            request.Name,
            role,
            request.Password,
            cancellationToken);

        if (result.Succeeded)
        {
            return CreatedAtAction(
                nameof(GetAll),
                new AdminUserMutationResponse
                {
                    UserId = result.UserId!.Value,
                    Status = "Active",
                });
        }

        return ConflictProblem(result.FailureReason!.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserMutationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseRole(request.Role, out var role))
        {
            return BadRequestProblem("Role must be Employee or Admin.");
        }

        var result = await userAdminService.UpdateUserAsync(
            id,
            request.Email,
            request.Name,
            role,
            cancellationToken);

        if (result.Succeeded)
        {
            return Ok(new AdminUserMutationResponse { UserId = id, Status = "Updated" });
        }

        return result.FailureReason switch
        {
            UserAdminFailureReason.NotFound => NotFoundProblem(result.FailureReason.Value),
            _ => ConflictProblem(result.FailureReason!.Value),
        };
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(AdminUserMutationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await userAdminService.DeactivateUserAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(new AdminUserMutationResponse { UserId = id, Status = "Inactive" });
        }

        return result.FailureReason switch
        {
            UserAdminFailureReason.NotFound => NotFoundProblem(result.FailureReason.Value),
            _ => ConflictProblem(result.FailureReason!.Value),
        };
    }

    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(typeof(AdminResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        [FromBody] AdminResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userAdminService.ResetPasswordAsync(id, request.NewPassword, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(new AdminResetPasswordResponse
            {
                UserId = id,
                Status = "Updated",
            });
        }

        return result.FailureReason switch
        {
            UserAdminFailureReason.InvalidPassword => BadRequestProblem("Password is required."),
            UserAdminFailureReason.NotFound => NotFoundProblem(result.FailureReason.Value),
            _ => ConflictProblem(result.FailureReason!.Value),
        };
    }

    private static AdminUserResponse MapUser(AdminUserListItem item) =>
        new()
        {
            UserId = item.UserId,
            Name = item.Name,
            Email = item.Email,
            Role = item.Role.ToString(),
            Status = item.IsActive ? "Active" : "Inactive",
            CanDeactivate = item.CanDeactivate,
            CanDemote = item.CanDemote,
        };

    private static bool TryParseRole(string role, out UserRole parsed) =>
        Enum.TryParse(role, ignoreCase: true, out parsed);

    private IActionResult ConflictProblem(UserAdminFailureReason reason) =>
        Problem(
            detail: UserApiMessages.Failure(reason),
            statusCode: StatusCodes.Status409Conflict,
            title: "User operation failed");

    private IActionResult NotFoundProblem(UserAdminFailureReason reason) =>
        Problem(
            detail: UserApiMessages.Failure(reason),
            statusCode: StatusCodes.Status404NotFound,
            title: "User not found");

    private IActionResult BadRequestProblem(string detail) =>
        Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest, title: "Invalid request");
}
