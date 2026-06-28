using System;

namespace MoneyTracker;

public class ExpenseQuerryParams
{
    public string? ExpenseCategory { get; set; }  
    public AmountRange? Amount { get; set; }
    public DateRange? Date { get; set; }
}
