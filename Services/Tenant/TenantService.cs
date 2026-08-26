using System;
using System.IdentityModel.Tokens.Jwt;

namespace MoneyTracker;

public class TenantService(
    IHttpContextAccessor httpContextAccessor,
    CatalogDbContext catalogDbContext,
    ILogger<TenantService> logger) 
    : ITenantService
{
    public string? GetCurrentTenantId()
    {
        var userEmail = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            logger.LogError("User email not found in the current context.");
            return null;
        }

        logger.LogInformation($"Retrieved User Email: {userEmail}");
        
        var tenantId = catalogDbContext.Tenants
            .Where(t => t.Users.Any(u => u.Email == userEmail))
            .Select(t => t.Id)
            .FirstOrDefault();

        return tenantId;
    }
}