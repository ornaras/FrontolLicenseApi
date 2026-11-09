namespace FrontolLicenseApi.Configurations;

public sealed class FrontolConfiguration
{
    public const string SectionName = "Frontol";

    public required string Login { get; set; }
    public required string Password { get; set; }
    public int PageSize { get; set; } = 100;
}
