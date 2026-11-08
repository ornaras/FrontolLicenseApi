using FrontolLicenseApi.Models;
using FrontolLicenseApi.Services;

namespace FrontolLicenseApi.Extensions;

public static class DbContextExtensions
{
    extension(ApplicationDbContext ctx)
    {
        public void UpsertLicense(License license)
        {
            if (license.Id.Contains('*')) return;
            var existing = ctx.Licenses.FirstOrDefault(l => l.Id == license.Id);
            if (existing is null) ctx.Licenses.Add(license);
            else
            {
                if (license.Type is not null) existing.Type = license.Type;
                if (license.ExpiredAt is not null) existing.ExpiredAt = license.ExpiredAt;
                if (license.ParentId is not null) existing.ParentId = license.ParentId;
            }
        }
    }
}
