using System;

namespace MoneyTracker;

public class MigrationTenantService(
    AppTenant userTenant
) : ITenantService
{
    public string? GetCurrentTenantSchemaName()
    {
       return userTenant.SchemaName;
    }
}
