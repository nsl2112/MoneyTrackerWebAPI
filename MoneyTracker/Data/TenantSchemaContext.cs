namespace MoneyTracker;

public sealed class TenantSchemaContext
{
    public string? SchemaName { get; private set; }

    public void SetSchemaName(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            schemaName.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
        {
            throw new ArgumentException("The tenant schema name contains invalid characters.", nameof(schemaName));
        }

        SchemaName = schemaName;
    }
}