using System.Text.Json.Serialization;

namespace FrontolLicenseApi.Models.Request;

public readonly struct FrontolDetailLicenseRequest(int id)
{
    [JsonPropertyName("id")] public int Id { get; } = id;
}
