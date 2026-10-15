using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace MoneyTracker;

public sealed class TenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
{
    private static readonly Regex SafeSchemaRegex = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    public string? GetCurrentTenantSchemaName()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        var schemaName = httpContext.Request.Headers["X-Tenant-Schema"].FirstOrDefault()
                         ?? httpContext.Request.Headers["schemaName"].FirstOrDefault()
                         ?? httpContext.User?.FindFirst("schemaName")?.Value;

        if (string.IsNullOrWhiteSpace(schemaName))
        {
            return null;
        }

        if (!SafeSchemaRegex.IsMatch(schemaName))
        {
            throw new ArgumentException("The tenant schema name contains invalid characters.", nameof(schemaName));
        }

        return schemaName;
    }
}