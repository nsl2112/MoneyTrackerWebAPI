using System;

namespace MoneyTracker;

public static class ExpenseExtension
{
    public static IQueryable<ExpenseItem> ApplyFilters(this IQueryable<ExpenseItem> query, ExpenseQuerryParams? queryParams)
    {
        if (queryParams != null)
        {
            if (!string.IsNullOrEmpty(queryParams.ExpenseCategory))
            {
                query = query.Where(e => e.ExpenseCategory.Name == queryParams.ExpenseCategory);
            }

            if (queryParams.Amount != null)
            {
                query = query.Where(e => e.Amount >= queryParams.Amount.MinAmount &&
                                         e.Amount <= queryParams.Amount.MaxAmount);
            }

            if (queryParams.Date != null)
            {
                query = query.Where(e => e.TransactionDate >= queryParams.Date.StartDate &&
                                         e.TransactionDate <= queryParams.Date.EndDate);
            }
        }

        return query;
    }
}
