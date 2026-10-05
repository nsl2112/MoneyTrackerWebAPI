using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace MoneyTracker;

public class TenantDbContext(
    DbContextOptions<TenantDbContext> options,
    IOptions<ConnectionStrings> connectionStringsOptions,
    ITenantService tenantService,
    TenantSchemaContext tenantSchemaContext)
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
        var schemaName = tenantSchemaContext.SchemaName ?? tenantService.GetCurrentTenantSchemaName();
        if (string.IsNullOrWhiteSpace(schemaName))
        {
            throw new InvalidOperationException("A tenant schema must be selected before using the database context.");
        }

        tenantSchemaContext.SetSchemaName(schemaName);

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionStringsOptions.Value.PostgreSqlConnection)
        {
            SearchPath = schemaName
        };

        optionsBuilder.UseNpgsql(connectionStringBuilder.ConnectionString);
    }
}
