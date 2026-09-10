namespace EmployeeDeskBooking.Infrastructure.Seed;

public enum SeedMode
{
    Minimal,
    Json,
    None,
}

public static class SeedOptions
{
    public const string SectionName = "Seed";

    public const string DefaultDatasetRelativePath = "data/dataset.json";

    public static SeedMode ParseMode(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "json" => SeedMode.Json,
            "none" => SeedMode.None,
            _ => SeedMode.Minimal,
        };
}
