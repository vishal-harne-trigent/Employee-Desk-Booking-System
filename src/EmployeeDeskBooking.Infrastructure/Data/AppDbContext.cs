using Microsoft.EntityFrameworkCore;

namespace EmployeeDeskBooking.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
