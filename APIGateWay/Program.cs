using Scalar.AspNetCore;
using MoneyTracker;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddScoped<ITokenService, JWTTokenService>();
    
builder.Services.AddOptions<JWT>().BindConfiguration(JWT.SectionName);
builder.Services.AddOptions<ConnectionStrings>().BindConfiguration(ConnectionStrings.SectionName);

builder.Services.AddDbContext<CatalogDbContext>();

builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<CatalogDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        NameClaimType = JwtRegisteredClaimNames.Name,
        RoleClaimType = "role",
    };
});

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        // 1. Strip client-supplied security & identity headers to prevent spoofing
        builderContext.AddRequestHeaderRemove("Authorization");
        builderContext.AddRequestHeaderRemove("X-User-Id");
        builderContext.AddRequestHeaderRemove("X-Tenant-Schema");
        builderContext.AddRequestHeaderRemove("X-User-Role");
        builderContext.AddRequestHeaderRemove("X-User-Email");
        builderContext.AddRequestHeaderRemove("schemaName");
        builderContext.AddRequestHeaderRemove("X-Internal-Key");

        // 2. Inject verified headers from authenticated HttpContext.User
        builderContext.AddRequestTransform(transformContext =>
        {
            var user = transformContext.HttpContext.User;

            // Propagate internal gateway secret for service-to-service authentication
            var internalKey = builder.Configuration["InternalAuth:SecretKey"] ?? "moneytracker-internal-gateway-secret-key";
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Internal-Key", internalKey);

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
                }

                var schemaName = user.FindFirst("schemaName")?.Value
                                 ?? user.FindFirst("schema")?.Value;
                if (!string.IsNullOrEmpty(schemaName))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Tenant-Schema", schemaName);
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("schemaName", schemaName);
                }

                var role = user.FindFirst("role")?.Value
                           ?? user.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(role))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Role", role);
                }

                var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                            ?? user.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);
                }
            }

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapReverseProxy();
app.Run();