using System.Globalization;
using System.Text.Json.Serialization;

namespace FrontolLicenseApi.Models.Response;

public readonly struct FrontolDetailLicenseResponse
{
    [JsonPropertyName("data")] public FrontolDetailLicense Data { get; init; }

    public readonly struct FrontolDetailLicense
    {
        [JsonPropertyName("lid")] public string LicenseId { get; init; }
        [JsonPropertyName("pid")] public string ProductId { get; init; }
        [JsonPropertyName("product_name")] public string ProductDisplayName { get; init; }
        [JsonPropertyName("subscription_date")] public string? SubscriptionDateString { get; init; }
        public DateOnly? SubscriptionDate => DateOnly.TryParseExact(SubscriptionDateString, "dd.MM.yyyy", out var date) ? date : null;
        [JsonPropertyName("lic_exp")] public int? LeftDays { get; init; }
        [JsonPropertyName("base_for")] public FrontolBaseForItem[]? BaseFor { get; init; }
        [JsonPropertyName("prolong_licenses")] public FrontolProlongLicense[]? ProlongLicenses { get; init; }
        [JsonPropertyName("subscriptions")] public FrontolSubscription[]? Subscriptions { get; init; }

        public readonly struct FrontolBaseForItem
        {
            [JsonPropertyName("lid")] public string LicenseId { get; init; }
        }

        public readonly struct FrontolProlongLicense
        {
            [JsonPropertyName("lid")] public string LicenseId { get; init; }
            [JsonPropertyName("operation_date")] public string OperationDateString { get; init; }
            public DateTime OperationDateTime => DateTime.ParseExact(OperationDateString, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
            [JsonPropertyName("remain_lifetime_new")] public int LeftDays { get; init; }
        }

        public readonly struct FrontolSubscription
        {
            [JsonPropertyName("lid")] public string LicenseId { get; init; }
            [JsonPropertyName("final_date")] public string FinalDateString { get; init; }
            public DateOnly FinalDate => DateOnly.ParseExact(FinalDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}
