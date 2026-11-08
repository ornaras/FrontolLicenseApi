namespace FrontolLicenseApi.Models;

public sealed class License
{
    public string Id { get; set; }
    public LicenseType? Type { get; set; }
    public DateOnly? ExpiredAt { get; set; }
    public string? Parent { get; set; }
    public LicenseRelationshipType? ParentType { get; set; }

    public enum LicenseRelationshipType
    {
        Converted,
        Prolong,
        Subscription
    }
}
