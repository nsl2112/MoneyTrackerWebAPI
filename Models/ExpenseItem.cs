using System;

namespace MoneyTracker;

public class ExpenseItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Description { get; set; }
    
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory ExpenseCategory { get; set; }
    
    public decimal Amount { get; set; }
   
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; }
    
    public DateTime TransactionDate { get; set; }
    
    public string? UserId { get; set; }
    public AppUser? AppUser { get; set; }
}
