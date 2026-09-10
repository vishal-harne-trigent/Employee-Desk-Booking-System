using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Api.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(User user);
}
