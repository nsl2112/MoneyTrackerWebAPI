using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker;

public class AppDbContext: IdentityDbContext
{
    public string TentantId { get; private set; } = null!; 
    public DbSet<ExpenseItem> ExpenseItems { get; set; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<IncomeItem> IncomeItems { get; set; }
    public DbSet<IncomeCategory> IncomeCategories { get; set; } 
    public DbSet<Currency> Currencies { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService) : base(options)
    {
        TentantId = tenantService.GetCurrentTenantId();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseItemTypeConfiguration());
        modelBuilder.ApplyConfiguration(new IncomeCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new IncomeItemConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }
}
