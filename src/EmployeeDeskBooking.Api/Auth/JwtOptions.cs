namespace EmployeeDeskBooking.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "EmployeeDeskBooking";

    public string Audience { get; set; } = "EmployeeDeskBooking.Api";

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 480;
}
