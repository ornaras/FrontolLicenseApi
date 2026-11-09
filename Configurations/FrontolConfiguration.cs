namespace FrontolLicenseApi.Configurations;

public sealed class FrontolConfiguration
{
    public const string SectionName = "Frontol";

    public string Login { get; set; }
    public string Password { get; set; }
    public int PageSize { get; set; }
}
