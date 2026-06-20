using System;

namespace MoneyTracker;

public class ExpenseCreateDTO
{
    public string Description { get; set; }
    public int ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class ExpenseGetDTO
{
    public string Description { get; set; }
    public string ExpenseCategoryName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyName { get; set; }
    public DateTime TransactionDate { get; set; }
}
