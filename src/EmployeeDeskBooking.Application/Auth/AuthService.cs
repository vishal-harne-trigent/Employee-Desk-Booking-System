using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Application.Users;

namespace EmployeeDeskBooking.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordVerifier _passwordVerifier;

    public AuthService(IUserRepository userRepository, IPasswordVerifier passwordVerifier)
    {
        _userRepository = userRepository;
        _passwordVerifier = passwordVerifier;
    }

    public async Task<LoginResult> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordVerifier.Verify(user, password))
        {
            return LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return LoginResult.Failure(LoginFailureReason.DeactivatedAccount);
        }

        return LoginResult.Success(user);
    }
}
