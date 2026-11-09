using FrontolLicenseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrontolLicenseApi.Services;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Product> Products => Set<Product>();

    public ApplicationDbContext(DbContextOptions opts) : base(opts)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<License>(e =>
        {
            e.HasOne(l => l.Product)
                .WithMany(t => t.Licenses)
                .HasForeignKey("ProductId");
            e.HasKey(l => l.Code);
        });
        base.OnModelCreating(modelBuilder);
    }
}
