using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MoneyTracker;

public sealed class TenantProvisioningService(
    IHttpClientFactory httpClientFactory) 
    : ITenantProvisioningService
{
    public async Task ProvisionAsync(
        AppTenant tenant,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "http://localhost:5079/api/TenantDb");

        request.Headers.Add("schemaName", tenant.SchemaName);
        
        var client = httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}