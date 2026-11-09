namespace FrontolLicenseApi.Models;

public sealed class Product
{
    public string Id { get; init; }
    public string DisplayName { get; init; }

    public IEnumerable<License> Licenses { get; init; }
}
