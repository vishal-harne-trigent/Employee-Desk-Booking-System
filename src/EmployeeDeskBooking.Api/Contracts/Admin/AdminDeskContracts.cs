namespace EmployeeDeskBooking.Api.Contracts.Admin;

public sealed class AdminDeskResponse
{
    public required Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public required string Status { get; init; }

    public required bool CanDeactivate { get; init; }
}

public sealed class AdminDesksListResponse
{
    public IReadOnlyList<AdminDeskResponse> Desks { get; init; } = Array.Empty<AdminDeskResponse>();
}

public sealed class CreateAdminDeskRequest
{
    public required string DeskNumber { get; init; }
}

public sealed class UpdateAdminDeskRequest
{
    public required string DeskNumber { get; init; }
}

public sealed class AdminDeskMutationResponse
{
    public required Guid DeskId { get; init; }

    public required string Status { get; init; }
}
