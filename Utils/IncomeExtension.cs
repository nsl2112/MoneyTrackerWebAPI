using System;

namespace MoneyTracker;

public static class IncomeExtension
{
    public static IQueryable<IncomeItem> ApplyFilters(this IQueryable<IncomeItem> query, IncomeQuerryParams? queryParams)
    {
        if (queryParams != null)
        {
            if (!string.IsNullOrEmpty(queryParams.Category))
            {
                query = query.Where(e => e.IncomeCategory.Name == queryParams.Category);
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
