using System;

namespace MoneyTracker;

public class ExpenseItem
{
    public string? Id { get; set; }
    public string Description { get; set; }
    public int ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public DateTime Date { get; set; }
    public string? UserId { get; set; }
}
