namespace EmployeeDeskBooking.Application.Auth;

public interface IAuthService
{
    Task<LoginResult> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
}
