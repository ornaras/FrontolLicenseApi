using FrontolLicenseApi.Models.Response;
using FrontolLicenseApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FrontolLicenseApi.Controllers;

[ApiController]
[Route("licenses")]
public sealed class LicensesController : Controller
{
    [HttpGet]
    public Results<Ok<GetAllLicensesResponse>, BadRequest<ErrorResponse>> GetAll
        (ApplicationDbContext ctx, 
        [FromQuery] int size = 100,
        [FromQuery] int page = 0,
        [FromQuery] string[]? types = null)
    {
        if (size <= 0 || size > 100)
            return (BadRequest<ErrorResponse>)Results.BadRequest(new ErrorResponse("Значение size должно находиться в диапазоне от 0 до 100 (включительно)"));
        if (page < 0)
            return (BadRequest<ErrorResponse>)Results.BadRequest(new ErrorResponse("Значение page должно быть не отрицательным."));

        IQueryable<Models.License> licenses = ctx.Licenses;
        if (types is { Length: > 0 } _types)
            licenses = licenses.Where(l => _types.Contains(l.Type));

        return (Ok<GetAllLicensesResponse>)Results.Ok(new GetAllLicensesResponse
        {
            TotalCount = ctx.Licenses.Count(),
            FilteredCount = licenses.Count(),
            Licenses = licenses
                .Skip(page * size).Take(size)
                .Select(l => new GetAllLicensesResponse.LicenseData(l))
        });
    }

    [HttpGet("types")]
    public IEnumerable<string> GetTypes(ApplicationDbContext ctx) =>
        ctx.Licenses.Select(l => l.Type).Distinct();
}
