using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MoneyTracker;

public class CatalogDbContext(
    DbContextOptions<CatalogDbContext> options,
    IOptions<ConnectionStrings> connectionStringsOptions) 
    : IdentityDbContext(options)
{
    public DbSet<AppTenant> Tenants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var connectionString = connectionStringsOptions.Value.PostgreSqlConnection;
        optionsBuilder.UseNpgsql(connectionString);
    }
}
