using FrontolLicenseApi.Configurations;
using FrontolLicenseApi.Models;
using FrontolLicenseApi.Models.Request;
using FrontolLicenseApi.Models.Response;
using FrontolLicenseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;

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

    [HttpPost("planfix")]
    [ProducesResponseType<QueryPlanfixResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public IResult QueryPlanfix(ApplicationDbContext ctx, IOptions<LocalesConfiguration> options, [FromBody] QueryPlanfixRequest body)
    {
        var locales = options.Value;
        var matches = LicenseCodeRegex().Matches(body.TaskDescription);
        if (matches.Count == 0) return Results.BadRequest(new ErrorResponse("В описании отсутствуют коды лицензий Frontol."));
        var builder = new StringBuilder(locales.LicenseInfoHeader);
        foreach(var licenseCode in matches.Select(l => l.Value))
        {
            var item = locales.LicenseInfoItem;
            item = item.Replace("{Code}", licenseCode);

            var license = ctx.Licenses.FirstOrDefault(l => l.Code == licenseCode);
            item = item.Replace("{ExpiredAt}", license?.ExpiredAt?.ToString("dd/MM/yyyy") ?? "-");

            if (license is null) item = item.Replace("{Status}", locales.LicenseStatusNotFound);
            else if (license.ExpiredAt is null || license.ExpiredAt.Value > DateOnly.FromDateTime(DateTime.Today))
                item = item.Replace("{Status}", locales.LicenseStatusHealthy);
            else item = item.Replace("{Status}", locales.LicenseStatusExpired);

            builder.Append(item);
        }
        return Results.Ok(new QueryPlanfixResponse(builder.ToString()));
    }

    [GeneratedRegex(@"([A-Za-z0-9]{5}-){3}[A-Za-z0-9]{5}")]
    private static partial Regex LicenseCodeRegex();
}
