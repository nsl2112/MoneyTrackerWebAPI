using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace MoneyTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantDbController(
        TenantDbContext context,
        TenantSchemaContext tenantSchemaContext,
        IOptions<ConnectionStrings> connectionStringsOptions)
        : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateTenantSchema([FromHeader] string schemaName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(schemaName))
            {
                return BadRequest("Schema name cannot be null or empty.");
            }

            string quotedSchema;
            try
            {
                quotedSchema = QuoteIdentifier(schemaName);
                tenantSchemaContext.SetSchemaName(schemaName);
            }
            catch (ArgumentException)
            {
                return BadRequest("The tenant schema name contains invalid characters.");
            }

            var connectionStrings = connectionStringsOptions.Value;
            var adminConnectionString = string.IsNullOrWhiteSpace(connectionStrings.PostgreSqlAdminConnection)
                ? connectionStrings.PostgreSqlConnection
                : connectionStrings.PostgreSqlAdminConnection;
            if (string.IsNullOrWhiteSpace(adminConnectionString))
            {
                throw new InvalidOperationException("The PostgreSQL connection string is not configured.");
            }

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(adminConnectionString)
            {
                SearchPath = "public"
            };

            await using (var adminConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString))
            {
                await adminConnection.OpenAsync(cancellationToken);

                await using var command = new NpgsqlCommand(
                    "CREATE SCHEMA IF NOT EXISTS " + quotedSchema,
                    adminConnection);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await context.Database.MigrateAsync(cancellationToken);

            return Ok($"Database for schema '{schemaName}' created successfully.");
        }

        private static string QuoteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) ||
                identifier.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
            {
                throw new ArgumentException("The tenant schema name contains invalid characters.", nameof(identifier));
            }

            return $"\"{identifier}\"";
        }
    }

    
}
