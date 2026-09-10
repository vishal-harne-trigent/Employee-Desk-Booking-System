using System.ComponentModel.DataAnnotations;

namespace EmployeeDeskBooking.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }

    public required string Role { get; init; }
}

public sealed class CurrentUserResponse
{
    public required string Email { get; init; }

    public required string Name { get; init; }

    public required string Role { get; init; }
}
