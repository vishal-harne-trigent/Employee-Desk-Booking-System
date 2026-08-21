using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Users;

namespace EmployeeDeskBooking.Application.Security;

public interface IPasswordVerifier
{
    string HashPassword(User user, string password);

    bool Verify(User user, string password);
}
