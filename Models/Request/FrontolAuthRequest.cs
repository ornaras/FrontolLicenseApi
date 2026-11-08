using System.Text.Json.Serialization;

namespace FrontolLicenseApi.Models.Request;

public readonly struct FrontolAuthRequest(string login, string password)
{
    [JsonPropertyName("email")] public string Login { get; } = login;
    [JsonPropertyName("password")] public string Password { get; } = password;
    [JsonPropertyName("adv_agree")] public bool AdvAgree { get; } = false;
}
