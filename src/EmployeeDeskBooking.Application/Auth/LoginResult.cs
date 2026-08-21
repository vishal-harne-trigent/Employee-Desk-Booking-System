using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Application.Auth;

public sealed class LoginResult
{
    private LoginResult(bool isSuccess, User? user, LoginFailureReason? failureReason)
    {
        IsSuccess = isSuccess;
        User = user;
        FailureReason = failureReason;
    }

    public bool IsSuccess { get; }

    public User? User { get; }

    public LoginFailureReason? FailureReason { get; }

    public static LoginResult Success(User user) => new(true, user, null);

    public static LoginResult Failure(LoginFailureReason reason) => new(false, null, reason);
}
