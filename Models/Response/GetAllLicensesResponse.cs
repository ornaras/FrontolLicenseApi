namespace FrontolLicenseApi.Models.Response;

public readonly struct GetAllLicensesResponse
{
    public IEnumerable<LicenseData> Licenses { get; init; }
    public int FilteredCount { get; init; }
    public int TotalCount { get; init; }

    public readonly struct LicenseData(License model)
    {
        public string License { get; } = model.Code;
        public string? ProductId { get; } = model.Product?.Id;
        public DateOnly? ExpiredAt { get; } = model.ExpiredAt;
        public string? ParentCode { get; } = model.ParentCode;
    }
}
