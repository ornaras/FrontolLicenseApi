using System.Text.Json.Serialization;

namespace FrontolLicenseApi.Models.Response;

public readonly struct FrontolLicensesPageResponse
{
    [JsonPropertyName("data")] public FrontolLicensesPageItem[] Data { get; init; }
    [JsonPropertyName("next_page")] public int? NextPage { get; init; }

    public readonly struct FrontolLicensesPageItem
    {
        [JsonPropertyName("id")] public int Id { get; init; }
        [JsonPropertyName("pid")] public string Type { get; init; }
    }

}
