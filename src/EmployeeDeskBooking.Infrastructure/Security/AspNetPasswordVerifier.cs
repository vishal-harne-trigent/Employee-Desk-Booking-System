using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace EmployeeDeskBooking.Infrastructure.Security;

public sealed class AspNetPasswordVerifier : IPasswordVerifier
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password) =>
        _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
            != PasswordVerificationResult.Failed;
}
