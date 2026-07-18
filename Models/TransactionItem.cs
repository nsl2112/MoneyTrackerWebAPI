using System;

namespace MoneyTracker;

public abstract class TransactionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Description { get; set; }
    
    public int TransactionCategoryId { get; set; }
    public TransactionCategory TransactionCategory { get; set; }
    
    public decimal Amount { get; set; }
   
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; }
    
    public DateTime TransactionDate { get; set; }
    
    public string? UserId { get; set; }
    public AppUser? AppUser { get; set; }
}

public class ExpenseItem : TransactionItem
{
    public new ExpenseCategory TransactionCategory {get; set;}
}

public class IncomeItem : TransactionItem
{
    public new IncomeCategory TransactionCategory {get; set;}
}