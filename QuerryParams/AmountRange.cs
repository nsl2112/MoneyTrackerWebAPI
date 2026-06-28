using System;

namespace MoneyTracker;

public class AmountRange : IParsable<AmountRange>
{
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public static AmountRange Parse(string? s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Invalid amount range format: '{s}'. Expected format: 'min-max' or a single value.");
        }

        return result;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out AmountRange result)
    {
        var values = s?.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (values?.Length == 1 && decimal.TryParse(values[0], provider, out var singleValue))
        {
            singleValue = Math.Round(singleValue, 2); // Round to 2 decimal places
            result = new AmountRange { MinAmount = singleValue, MaxAmount = singleValue };
            return true;
        }

        if (values?.Length == 2 &&
            decimal.TryParse(values[0], provider, out var minValue) &&
            decimal.TryParse(values[1], provider, out var maxValue))
        {
            if (minValue > maxValue) // Swap if minValue is greater than maxValue
            {
                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            minValue = Math.Round(minValue, 2);
            maxValue = Math.Round(maxValue, 2);

            result = new AmountRange { MinAmount = minValue, MaxAmount = maxValue };
            return true;
        }

        result = new AmountRange { MinAmount = default, MaxAmount = default };
        return false;
    }
}
