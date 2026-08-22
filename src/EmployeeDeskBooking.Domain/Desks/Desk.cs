namespace EmployeeDeskBooking.Domain.Desks;

public class Desk
{
    public Guid Id { get; set; }

    public string DeskNumber { get; set; } = string.Empty;

    public string DeskNumberNormalized { get; set; } = string.Empty;

    public DeskStatus Status { get; set; }

    public string Location { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
