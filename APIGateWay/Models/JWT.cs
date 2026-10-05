using System;

namespace MoneyTracker;

public class JWT
{
    public const string SectionName = "JWT";
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public double DurationInMinutes { get; set; }
}
