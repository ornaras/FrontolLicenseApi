namespace FrontolLicenseApi.Models.Response;

public readonly struct QueryPlanfixResponse(string msg)
{
    public string CommentMessage { get; } = msg;
}
