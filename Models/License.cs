namespace FrontolLicenseApi.Models;

public sealed class License
{
    public string Code { get; set; }
    public Product? Product { get; set; }
    public DateOnly? ExpiredAt { get; set; }
    public string? ParentCode { get; set; }
    public LicenseRelationshipType? ParentType { get; set; }

    public enum LicenseRelationshipType
    {
        Converted,
        Prolong,
        Subscription
    }
}
