namespace MoneyTracker;

public class ConnectionStrings
{
    public const string SectionName = "ConnectionStrings";
    public string PostgreSqlConnection { get; set; }
    public string PostgreSqlAdminConnection { get; set; } = string.Empty;
}
