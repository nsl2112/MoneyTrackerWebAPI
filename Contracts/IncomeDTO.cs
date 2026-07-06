using System;

namespace MoneyTracker;

public class IncomeCreateDTO
{
    public string Description { get; set; }
    public int IncomeCategoryId { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class IncomeGetDTO
{
    public string Description { get; set; }
    public string IncomeCategoryName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyName { get; set; }
    public DateTime TransactionDate { get; set; }
}
