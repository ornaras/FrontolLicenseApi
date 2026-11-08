namespace FrontolLicenseApi.Models;

public sealed class License
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public DateOnly? ExpiredAt { get; init; }
    public string? ParentId { get; init; }
    public License? Parent { get; init; }
}
