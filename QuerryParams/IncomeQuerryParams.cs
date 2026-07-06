using System;

namespace MoneyTracker;

public class IncomeQuerryParams
{
    public string? Category {get; set;}
    public DateRange? DateRange {get; set;}
    public AmountRange? AmountRange {get; set;}
}
