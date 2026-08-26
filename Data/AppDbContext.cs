using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MoneyTracker;

public class TenantDbContext(
    DbContextOptions<TenantDbContext> options,
    IOptions<ConnectionStrings> connectionStringsOptions,
    ITenantService tenantService)
    : DbContext(options)
{
    public DbSet<ExpenseItem> ExpenseItems { get; set; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<IncomeItem> IncomeItems { get; set; }
    public DbSet<IncomeCategory> IncomeCategories { get; set; } 
    public DbSet<Currency> Currencies { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseItemTypeConfiguration());
        modelBuilder.ApplyConfiguration(new IncomeCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new IncomeItemConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(connectionStringsOptions.Value.PostgreSqlConnection 
            + $"; Search Path=Tenant_{tenantService.GetCurrentTenantId()};");
    }
}
