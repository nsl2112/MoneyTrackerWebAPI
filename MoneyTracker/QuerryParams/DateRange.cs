using System;

namespace MoneyTracker;

public class DateRange : IParsable<DateRange>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public static DateRange Parse(string? s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Invalid date range format: '{s}'. Expected format: 'yyyy-MM-dd_yyyy-MM-dd'.");
        }

        return result;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out DateRange result)
    {
        var segments = s?.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (segments?.Length == 1 && 
            DateTime.TryParse(segments[0], provider, out var singleDate))
        {
            result = new DateRange {StartDate = singleDate, EndDate = singleDate.AddDays(1)};
            return true;
        }

        if (segments?.Length == 2 && 
            DateTime.TryParse(segments[0], provider, out var startDate) &&
            DateTime.TryParse(segments[1], provider, out var endDate))
        {
            if (startDate > endDate) //Swap if startDate is greater than endDate
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            result = new DateRange {StartDate = startDate, EndDate = endDate};
            return true;
        }

        result = new DateRange {StartDate = default, EndDate = default};
        return false;
    }   
}
