namespace FrontolLicenseApi.Models.Response;

public readonly struct ErrorResponse(string msg)
{
    public string Message { get; } = msg;
}
