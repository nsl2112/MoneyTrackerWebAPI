using System;
using System.Globalization;

namespace MoneyTracker;

public static class Utils
{
    public static string ConvertTimeValue(string timePeriod, DateTime timeValue)
    {
        return timePeriod switch
        {
            "day" => timeValue.ToString("yyyy-MM-dd"),
            "week" => $"{timeValue.Year}-W{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(timeValue, CalendarWeekRule.FirstDay, DayOfWeek.Monday)}",
            "month" => new DateTime(timeValue.Year, timeValue.Month, 1).ToString("yyyy-MM"),
            "year" => new DateTime(timeValue.Year, 1, 1).ToString("yyyy"),
            _ => string.Empty,
        };
    }
}
