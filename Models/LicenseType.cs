using System.ComponentModel.DataAnnotations;

namespace FrontolLicenseApi.Models;

public sealed class LicenseType
{
    [Key] public string Type { get; init; }
    public string DisplayName { get; init; }

    public IEnumerable<License> Licenses { get; init; }
}
