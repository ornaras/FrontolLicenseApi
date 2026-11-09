using FrontolLicenseApi.Models;
using FrontolLicenseApi.Services;

namespace FrontolLicenseApi.Extensions;

public static class DbContextExtensions
{
    extension(ApplicationDbContext ctx)
    {
        public void UpsertLicense(License license)
        {
            if (license.Code.Contains('*')) return;
            var existing = ctx.Licenses.FirstOrDefault(l => l.Code == license.Code);
            if (existing is null) ctx.Licenses.Add(license);
            else
            {
                if (license.Product is not null) existing.Product = license.Product;
                if (license.ExpiredAt is not null) existing.ExpiredAt = license.ExpiredAt;
                if (license.ParentCode is not null) existing.ParentCode = license.ParentCode;
            }
        }
    }
}
