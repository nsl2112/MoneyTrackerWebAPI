using System;

namespace MoneyTracker;

public class ExpenseQuerryParams
{
    public string? ExpenseCategory { get; set; }  
    public string? Amount { get; set; }
    public string? Date { get; set; }
}
