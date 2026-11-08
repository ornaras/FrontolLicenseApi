using System.Text.Json.Serialization;

namespace FrontolLicenseApi.Models.Response;

public readonly struct FrontolAuthResponse
{
    [JsonPropertyName("data")] public FrontolAuthData Data { get; init; }

    public readonly struct FrontolAuthData
    {
        [JsonPropertyName("auth_token")] public string AuthToken { get; init; }
    }
}
