using FrontolLicenseApi.Models;
using FrontolLicenseApi.Models.Response;
using FrontolLicenseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrontolLicenseApi.Controllers;

[ApiController]
[Route("licenses")]
public sealed partial class LicensesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetAllLicensesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public IResult GetAllLicenses(ApplicationDbContext ctx, [FromQuery] int size = 100, [FromQuery] int page = 0, [FromQuery] string[]? products = null)
    {
        if (size <= 0 || size > 100)
            return Results.BadRequest(new ErrorResponse("Значение size должно находиться в диапазоне от 0 до 100 (включительно)"));
        if (page < 0)
            return Results.BadRequest(new ErrorResponse("Значение page должно быть не отрицательным."));

        IQueryable<License> licenses = ctx.Licenses;
        if (products is { Length: > 0 } _products)
            licenses = licenses.Where(l => l.Product != null && _products.Contains(l.Product.Id));

        return Results.Ok(new GetAllLicensesResponse
        {
            TotalCount = ctx.Licenses.Count(),
            FilteredCount = licenses.Count(),
            Licenses = licenses
                .Skip(page * size).Take(size)
                .Select(l => new GetAllLicensesResponse.LicenseData(l))
        });
    }

    [HttpGet("products")]
    public IEnumerable<Product> GetProducts(ApplicationDbContext ctx) => ctx.Products;
}
