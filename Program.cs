using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MoneyTracker API...");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddSerilog((servies, lc) =>
    {   
        lc.ReadFrom.Configuration(builder.Configuration)
          .ReadFrom.Services(servies)
          .Enrich.FromLogContext()
          .WriteTo.Console();
    });

    builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"))
               .LogTo(Console.WriteLine, LogLevel.Information));

    builder.Services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>();
    
    builder.Services.AddOptions<JWT>().BindConfiguration(JWT.SectionName);
    builder.Services.AddScoped<ITokenService, JWTTokenService>();
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
    
    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

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

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal("An unhandled exception occurred: {ExceptionMessage}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}