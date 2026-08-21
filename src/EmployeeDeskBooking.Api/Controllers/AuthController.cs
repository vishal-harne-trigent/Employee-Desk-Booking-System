using System.Security.Claims;
using EmployeeDeskBooking.Api.Auth;
using EmployeeDeskBooking.Api.Contracts.Auth;
using EmployeeDeskBooking.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDeskBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IJwtTokenService jwtTokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await authService.SignInAsync(request.Email, request.Password, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.FailureReason switch
            {
                LoginFailureReason.DeactivatedAccount => Problem(
                    detail: ApiAuthMessages.DeactivatedAccount,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Account deactivated"),
                _ => Problem(
                    detail: ApiAuthMessages.InvalidCredentials,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials"),
            };
        }

        var user = result.User!;
        var (token, expiresAt) = jwtTokenService.CreateToken(user);

        return Ok(new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
        });
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse
        {
            Email = email,
            Name = name,
            Role = role,
        });
    }
}
