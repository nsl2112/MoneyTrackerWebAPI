using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MoneyTracker;

public class GatewayHeaderAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "GatewayHeaderAuth";
}

public class GatewayHeaderAuthenticationHandler : AuthenticationHandler<GatewayHeaderAuthenticationOptions>
{
    private readonly IConfiguration _configuration;

    public GatewayHeaderAuthenticationHandler(
        IOptionsMonitor<GatewayHeaderAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Verify internal gateway secret
        var internalKey = Request.Headers["X-Internal-Key"].FirstOrDefault();
        var expectedSecret = _configuration["InternalAuth:SecretKey"] ?? "moneytracker-internal-gateway-secret-key";

        if (string.IsNullOrEmpty(internalKey) || internalKey != expectedSecret)
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid internal gateway key. Direct downstream requests are forbidden."));
        }

        // 2. Extract user identity headers
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(userId))
        {
            // Anonymous / non-user request within internal network
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var email = Request.Headers["X-User-Email"].FirstOrDefault();
        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        var role = Request.Headers["X-User-Role"].FirstOrDefault();
        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim("role", role));
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var schemaName = Request.Headers["X-Tenant-Schema"].FirstOrDefault() 
                         ?? Request.Headers["schemaName"].FirstOrDefault();
        if (!string.IsNullOrEmpty(schemaName))
        {
            claims.Add(new Claim("schemaName", schemaName));
            claims.Add(new Claim("schema", schemaName));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name, JwtRegisteredClaimNames.Name, "role");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
