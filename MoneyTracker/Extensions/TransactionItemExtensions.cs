using System;

namespace MoneyTracker;

public static class ExpenseExtension
{
    public static IQueryable<TransactionItem> ApplyFilters(this IQueryable<TransactionItem> query, TransactionQueryParams? queryParams)
    {
        if (queryParams != null)
        {
            if (!string.IsNullOrEmpty(queryParams.Category))
            {
                query = query.Where(e => e.TransactionCategory.Name == queryParams.Category);
            }

            if (queryParams.AmountRange != null)
            {
                query = query.Where(e => e.Amount >= queryParams.AmountRange.MinAmount &&
                                         e.Amount <= queryParams.AmountRange.MaxAmount);
            }

            if (queryParams.DateRange != null)
            {
                query = query.Where(e => e.TransactionDate >= queryParams.DateRange.StartDate &&
                                         e.TransactionDate <= queryParams.DateRange.EndDate);
            }
        }

        return query;
    }
}
