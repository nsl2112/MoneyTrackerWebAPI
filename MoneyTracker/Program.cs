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

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ITenantService, TenantService>();
    
    builder.Services.AddOptions<ConnectionStrings>().BindConfiguration(ConnectionStrings.SectionName);
    builder.Services.AddScoped<TenantSchemaContext>();
    builder.Services.AddDbContext<TenantDbContext>();
    
    builder.Services.AddAuthentication(GatewayHeaderAuthenticationOptions.DefaultScheme)
        .AddScheme<GatewayHeaderAuthenticationOptions, GatewayHeaderAuthenticationHandler>(
            GatewayHeaderAuthenticationOptions.DefaultScheme, null);

    builder.Services.AddAuthorization();
    
    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddKeyedScoped<ITransaction, TransactionService<ExpenseItem>>("Expense");
    builder.Services.AddKeyedScoped<ITransaction, TransactionService<IncomeItem>>("Income");
    builder.Services.AddScoped<ICurrencyService, CurrencyService>();
    builder.Services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
    builder.Services.AddScoped<IIncomeCategoryService, IncomeCategoryService>();

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