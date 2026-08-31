using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Infrastructure.Data;

internal static class DatabaseBootstrap
{
    public static void ConfigureDbContext(
        DbContextOptionsBuilder options,
        IConfiguration configuration)
    {
        if (UsesSqlite(configuration))
        {
            var connectionString = ResolveSqliteConnectionString(configuration);
            options.UseSqlite(EnsureSqliteDirectory(connectionString));
            return;
        }

        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
    }

    public static async Task InitializeAsync(AppDbContext dbContext, IConfiguration configuration)
    {
        if (UsesSqlite(configuration))
        {
            await dbContext.Database.EnsureCreatedAsync();
            return;
        }

        await dbContext.Database.MigrateAsync();
    }

    public static bool UsesSqlite(IConfiguration configuration) =>
        string.Equals(
            configuration[$"{DatabaseProviderOptions.SectionName}:Provider"],
            DatabaseProviderOptions.Sqlite,
            StringComparison.OrdinalIgnoreCase);

    private static string ResolveSqliteConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString("Sqlite")
        ?? DatabaseProviderOptions.DefaultSqliteConnectionString;

    private static string EnsureSqliteDirectory(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            return connectionString;
        }

        var fullPath = Path.GetFullPath(builder.DataSource);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        builder.DataSource = fullPath;
        return builder.ConnectionString;
    }
}
