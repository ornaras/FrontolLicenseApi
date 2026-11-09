namespace FrontolLicenseApi.Configurations;

public sealed class LocalesConfiguration
{
    public const string SectionName = "Locales";

    public required string LicenseInfoHeader { get; set; }
    public required string LicenseInfoItem { get; set; }
    public required string LicenseStatusNotFound { get; set; }
    public required string LicenseStatusExpired { get; set; }
    public required string LicenseStatusHealthy { get; set; }
}
