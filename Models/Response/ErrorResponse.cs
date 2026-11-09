namespace FrontolLicenseApi.Models.Response;

public readonly struct ErrorResponse(string msg)
{
    public string ErrorMessage { get; } = msg;
}
