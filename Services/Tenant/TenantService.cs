using System;

namespace MoneyTracker;

public class TenantService(IHttpContextAccessor httpContextAccessor, ILogger<TenantService> logger) : ITenantService
{
    public string GetCurrentTenantId()
    {
        var tenantId = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            logger.LogError("Tenant ID not found in the current context.");
            return string.Empty;
        }

        logger.LogInformation($"Retrieved Tenant ID: {tenantId}");

        return tenantId;
    }
}