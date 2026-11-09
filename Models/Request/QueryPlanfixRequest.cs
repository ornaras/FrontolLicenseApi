using System.Text.Json.Serialization;

namespace FrontolLicenseApi.Models.Request;

public readonly struct QueryPlanfixRequest
{
    [JsonPropertyName("desc")] public string TaskDescription { get; init; }
}
