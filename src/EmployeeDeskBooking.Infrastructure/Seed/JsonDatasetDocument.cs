namespace EmployeeDeskBooking.Infrastructure.Seed;

public sealed class JsonDatasetDocument
{
    public int Version { get; set; } = 1;

    public string DefaultPassword { get; set; } = DbInitializer.DefaultPassword;

    public List<JsonDatasetUser> Users { get; set; } = [];

    public List<JsonDatasetDesk> Desks { get; set; } = [];

    public List<JsonDatasetBooking> Bookings { get; set; } = [];

    public List<JsonDatasetNotificationPreference> NotificationPreferences { get; set; } = [];
}

public sealed class JsonDatasetUser
{
    public required string Email { get; set; }

    public required string Name { get; set; }

    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Password { get; set; }
}

public sealed class JsonDatasetDesk
{
    public required string DeskNumber { get; set; }

    public required string Status { get; set; }

    public string? Location { get; set; }
}

public sealed class JsonDatasetBooking
{
    public required string UserEmail { get; set; }

    public required string DeskNumber { get; set; }

    public int WorkingDayOffset { get; set; }

    public required string Status { get; set; }
}

public sealed class JsonDatasetNotificationPreference
{
    public required string UserEmail { get; set; }

    public bool PushOptIn { get; set; }
}
