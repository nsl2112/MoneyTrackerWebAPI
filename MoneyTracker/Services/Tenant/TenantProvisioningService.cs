using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MoneyTracker;

public sealed class TenantProvisioningService(
    IOptions<ConnectionStrings> connectionStringsOptions) 
    : ITenantProvisioningService
{
    public async Task ProvisionAsync(
        AppTenant tenant,
        CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>().Options;

        await using var tenantDbContext = new TenantDbContext(
            options,
            connectionStringsOptions,
            new MigrationTenantService(tenant));

        try
        {
            await tenantDbContext.Database.ExecuteSqlRawAsync(
                "CREATE SCHEMA IF NOT EXISTS " + QuoteIdentifier(tenant.SchemaName),
                cancellationToken);

            await tenantDbContext.Database.MigrateAsync(cancellationToken);
        }
        catch
        {
            await tenantDbContext.Database.ExecuteSqlRawAsync(
                "DROP SCHEMA IF EXISTS " + QuoteIdentifier(tenant.SchemaName) + " CASCADE",
                CancellationToken.None);
            throw;
        }
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) ||
            identifier.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
        {
            throw new ArgumentException("The tenant schema name contains invalid characters.", nameof(identifier));
        }

        return $"\"{identifier}\"";
    }
}