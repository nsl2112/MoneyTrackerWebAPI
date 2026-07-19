using System;

namespace MoneyTracker;

public class TransactionCreateDTO
{
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class TransactionGetDTO
{
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyName { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class TransactionTotalByCategoryDTO
{
    public string CategoryName { get; set; }
    public decimal TotalAmount { get; set; }
}

public class TransactionTotalByTimeDTO
{
    public DateTime TimePeriod { get; set; }
    public decimal TotalAmount { get; set; }
}

public class TransactionTotalByTimeStringDTO
{
    public string TimePeriod {get; set;}
    public decimal TotalAmount {get; set;}
}
