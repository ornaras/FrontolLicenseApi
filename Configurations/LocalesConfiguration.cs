namespace FrontolLicenseApi.Configurations;

public sealed class LocalesConfiguration
{
    public const string SectionName = "Locales";

    public string LicenseInfoHeader { get; set; }
    public string LicenseInfoItem { get; set; }
    public string LicenseStatusNotFound { get; set; }
    public string LicenseStatusExpired { get; set; }
    public string LicenseStatusHealthy { get; set; }
}
