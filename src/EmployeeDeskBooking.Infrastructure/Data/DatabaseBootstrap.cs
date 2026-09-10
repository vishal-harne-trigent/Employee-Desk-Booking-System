using EmployeeDeskBooking.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmployeeDeskBooking.Infrastructure.Data;

internal static class DatabaseBootstrap
{
    public static void ConfigureDbContext(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var provider = configuration[$"{DatabaseProviderOptions.SectionName}:Provider"];

        if (UsesInMemory(configuration))
        {
            options.UseInMemoryDatabase(DatabaseProviderOptions.InMemoryDatabaseName);
            return;
        }

        if (UsesSqlite(configuration))
        {
            options.UseSqlite(ResolveSqliteConnectionString(configuration));
            return;
        }

        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
    }

    public static async Task InitializeAsync(AppDbContext dbContext, IConfiguration configuration)
    {
        if (UsesInMemory(configuration))
        {
            await dbContext.Database.EnsureCreatedAsync();
            return;
        }

        await dbContext.Database.MigrateAsync();
    }

    public static bool UsesInMemory(IConfiguration configuration) =>
        string.Equals(
            configuration[$"{DatabaseProviderOptions.SectionName}:Provider"],
            DatabaseProviderOptions.InMemory,
            StringComparison.OrdinalIgnoreCase);

    public static bool UsesSqlite(IConfiguration configuration) =>
        string.Equals(
            configuration[$"{DatabaseProviderOptions.SectionName}:Provider"],
            DatabaseProviderOptions.Sqlite,
            StringComparison.OrdinalIgnoreCase);

    internal static string ResolveSqliteConnectionString(IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("Sqlite")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Sqlite is required when Database:Provider is Sqlite.");

        const string prefix = "Data Source=";
        if (!raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return raw;
        }

        var pathPart = raw[prefix.Length..].Split(';')[0].Trim();
        if (Path.IsPathRooted(pathPart))
        {
            EnsureSqliteDirectory(pathPart);
            return raw;
        }

        var fullPath = Path.GetFullPath(Path.Combine(JsonDatasetSeeder.ResolveRepoRoot(), pathPart));
        EnsureSqliteDirectory(fullPath);
        return $"{prefix}{fullPath}";
    }

    private static void EnsureSqliteDirectory(string databaseFilePath)
    {
        var directory = Path.GetDirectoryName(databaseFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
