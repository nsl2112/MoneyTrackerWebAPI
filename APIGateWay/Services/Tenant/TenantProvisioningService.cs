using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MoneyTracker;

public sealed class TenantProvisioningService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
    : ITenantProvisioningService
{
    public async Task ProvisionAsync(
        AppTenant tenant,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Downstream:MoneyTrackerUrl"] ?? "http://localhost:5079";
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/TenantDb");

        var internalKey = configuration["InternalAuth:SecretKey"] ?? "moneytracker-internal-gateway-secret-key";
        request.Headers.Add("X-Internal-Key", internalKey);
        request.Headers.Add("X-Tenant-Schema", tenant.SchemaName);
        request.Headers.Add("schemaName", tenant.SchemaName);
        
        var client = httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}