using EmployeeDeskBooking.Application;
using EmployeeDeskBooking.Infrastructure;
using EmployeeDeskBooking.Infrastructure.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Infra = EmployeeDeskBooking.Infrastructure.DependencyInjection;

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

var command = args[0].Trim().ToLowerInvariant();
var reset = args.Any(arg => arg is "--reset" or "-r");
string? datasetPath = null;

for (var i = 1; i < args.Length; i++)
{
    if (args[i] is "--file" or "-f" && i + 1 < args.Length)
    {
        datasetPath = args[++i];
        continue;
    }

    if (args[i] is "--reset" or "-r")
    {
        reset = true;
    }
}

var webProjectPath = ResolveWebProjectPath();
var configuration = BuildConfiguration(webProjectPath, command, reset);

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddSingleton<IConfiguration>(configuration);
services.AddApplication();
services.AddInfrastructure(configuration, enableReminderJob: false, enableCompletionJob: false);

var provider = services.BuildServiceProvider();

try
{
    Console.WriteLine($"Seed command: {command}");
    var providerName = configuration["Database:Provider"] ?? "SqlServer";
    Console.WriteLine($"Database provider: {providerName}");
    var connection = string.Equals(providerName, "Sqlite", StringComparison.OrdinalIgnoreCase)
        ? configuration.GetConnectionString("Sqlite")
        : configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Connection: {connection}");

    switch (command)
    {
        case "json":
            await InitializeDatabaseAsync(provider, runSeed: false);
            await JsonDatasetSeeder.SeedAsync(
                provider,
                resetPasswordsInDevelopment: true,
                replaceExistingData: reset,
                datasetPath: datasetPath);
            Console.WriteLine($"Dataset loaded from {(datasetPath ?? JsonDatasetSeeder.ResolveDatasetPath(configuration))}.");
            break;

        case "minimal":
            await InitializeDatabaseAsync(provider, runSeed: false);
            await DbInitializer.SeedMinimalAsync(provider, resetDefaultPasswordsInDevelopment: true);
            Console.WriteLine("Minimal seed applied (default admin, employee, and desks).");
            break;

        case "none":
            await InitializeDatabaseAsync(provider, runSeed: false);
            Console.WriteLine("Database initialized with no seed data.");
            break;

        case "init":
            await InitializeDatabaseAsync(provider, runSeed: true);
            Console.WriteLine($"Database initialized using Seed:Mode = {configuration["Seed:Mode"] ?? "Minimal"}.");
            break;

        default:
            Console.Error.WriteLine($"Unknown command '{command}'.");
            PrintHelp();
            return 1;
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed: {ex.Message}");
    return 1;
}

static async Task InitializeDatabaseAsync(IServiceProvider provider, bool runSeed) =>
    await Infra.InitializeDatabaseAsync(provider, isDevelopment: true, runSeed: runSeed);

static IConfiguration BuildConfiguration(string webProjectPath, string command, bool reset)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(webProjectPath)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddJsonFile("appsettings.Development.local.json", optional: true)
        .AddUserSecrets("EmployeeDeskBooking.Web-dev")
        .AddEnvironmentVariables();

    var seedMode = command switch
    {
        "json" => "Json",
        "minimal" => "Minimal",
        "none" => "None",
        _ => null,
    };

    var overrides = new Dictionary<string, string?>();
    if (seedMode is not null)
    {
        overrides["Seed:Mode"] = seedMode;
    }

    if (reset && command == "json")
    {
        overrides["Seed:JsonReplaceExisting"] = "true";
    }

    if (overrides.Count > 0)
    {
        builder.AddInMemoryCollection(overrides);
    }

    return builder.Build();
}

static void PrintHelp()
{
    Console.WriteLine("""
        Employee Desk Booking — database seed tool

        Usage:
          dotnet run --project tools/SeedDatabase -- <command> [options]

        Commands:
          json       Load data/dataset.json into the database
          minimal    Seed only default admin, employee, and A-01..A-05 desks
          none       Migrate/create schema only — keep or create empty tables
          init       Migrate + seed using Seed:Mode from appsettings.Development.json

        Options:
          --reset, -r          Replace existing rows when seeding from JSON
          --file, -f <path>    Custom dataset JSON path

        Examples:
          dotnet run --project tools/SeedDatabase -- json
          dotnet run --project tools/SeedDatabase -- json --reset
          dotnet run --project tools/SeedDatabase -- minimal
          dotnet run --project tools/SeedDatabase -- init
        """);
}

static string ResolveWebProjectPath()
{
    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "EmployeeDeskBooking.Web")),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "src", "EmployeeDeskBooking.Web")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "EmployeeDeskBooking.Web")),
    };

    foreach (var candidate in candidates)
    {
        if (Directory.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new DirectoryNotFoundException("Could not locate src/EmployeeDeskBooking.Web.");
}
