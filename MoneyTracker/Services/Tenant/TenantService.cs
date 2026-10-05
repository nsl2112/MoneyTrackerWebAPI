namespace MoneyTracker;

public sealed class TenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
{
    public string? GetCurrentTenantSchemaName()
    {
        return httpContextAccessor.HttpContext?.Request.Headers["schemaName"].FirstOrDefault();
    }
}