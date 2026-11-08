using FrontolLicenseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrontolLicenseApi.Services;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<License> Licenses => Set<License>();

    public ApplicationDbContext(DbContextOptions opts) : base(opts)
    {
        Database.EnsureCreated();
    }
}
