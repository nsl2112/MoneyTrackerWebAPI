using System;

namespace MoneyTracker;

public class TransactionQueryParams
{
    public string? Category {get; set;}
    public DateRange? DateRange {get; set;}
    public AmountRange? AmountRange {get; set;}
}
