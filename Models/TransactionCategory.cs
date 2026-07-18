using System;

namespace MoneyTracker;

public class TransactionCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ExpenseCategory : TransactionCategory
{

}

public class IncomeCategory : TransactionCategory
{

}