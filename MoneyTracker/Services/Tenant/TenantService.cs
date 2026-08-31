using System;
using System.IdentityModel.Tokens.Jwt;

namespace MoneyTracker;

public class TenantService(
    IHttpContextAccessor httpContextAccessor,
    CatalogDbContext catalogDbContext,
    ILogger<TenantService> logger) 
    : ITenantService
{
    public string? GetCurrentTenantSchemaName()
    {
        var userEmail = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            logger.LogError("User email not found in the current context.");
            return null;
        }

        logger.LogInformation($"Retrieved User Email: {userEmail}");

        var schemaName = catalogDbContext.Tenants
            .Where(t => t.Users.Any(u => u.Email == userEmail))
            .Select(t => t.SchemaName)
            .FirstOrDefault();

        return schemaName;
    }
}