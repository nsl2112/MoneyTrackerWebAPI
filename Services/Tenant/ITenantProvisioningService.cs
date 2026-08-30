namespace MoneyTracker;

public interface ITenantProvisioningService
{
    Task ProvisionAsync(AppTenant tenant, CancellationToken cancellationToken = default);
}