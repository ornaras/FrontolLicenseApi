using FrontolLicenseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrontolLicenseApi.Services;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<License> Licenses => Set<License>();
    public DbSet<LicenseType> Types => Set<LicenseType>();

    public ApplicationDbContext(DbContextOptions opts) : base(opts)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<License>(e =>
        {
            e.HasOne(l => l.Type).WithMany(t => t.Licenses).HasForeignKey("Type");
        });
        base.OnModelCreating(modelBuilder);
    }
}
