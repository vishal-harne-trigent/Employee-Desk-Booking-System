namespace EmployeeDeskBooking.Infrastructure.Data;

public static class DatabaseProviderOptions
{
    public const string SectionName = "Database";

    public const string SqlServer = "SqlServer";
    public const string Sqlite = "Sqlite";

    public const string DefaultSqliteConnectionString =
        "Data Source=../../data/employeedeskbooking.db";
}
