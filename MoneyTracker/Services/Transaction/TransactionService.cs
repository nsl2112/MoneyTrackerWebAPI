using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MoneyTracker;

public class TransactionService<T>(TenantDbContext context) : ITransaction 
    where T : TransactionItem, new()
{
    public async Task<TransactionItem> CreateTransactionAsync(
        string userID, 
        TransactionCreateDTO transactionCreateDTO)
    {
        var transaction = new T
        {
            Description = transactionCreateDTO.Description,
            TransactionCategoryId = transactionCreateDTO.CategoryId,
            Amount = transactionCreateDTO.Amount,
            CurrencyId = transactionCreateDTO.CurrencyId,
            TransactionDate = transactionCreateDTO.TransactionDate,
            UserId = userID
        };
        context.Set<T>().Add(transaction);
        await context.SaveChangesAsync();

        return transaction;
    }

    public async Task<int> DeleteTransactionAsync(string id)
    {
        return await context.Set<T>()
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync();;
    }

    public async Task<TransactionTotalByCategoryDTO> GetTotalTransactionAsync(
        [FromQuery] TransactionQueryParams? queryParams)
    {
        var totalAmount = await context.Set<T>()
            .ApplyFilters(queryParams)
            .SumAsync(e => e.Amount);
        
        return new TransactionTotalByCategoryDTO
        {
            CategoryName = queryParams?.Category ?? "All",
            TotalAmount = totalAmount
        };
    }

    public async Task<IEnumerable<TransactionTotalByCategoryDTO>> GetTotalTransactionByCategoryAsync(
        [FromQuery] TransactionQueryParams? queryParams)
    {
        var totalTransactionByCategory = await context.Set<T>()
            .Include(e => e.TransactionCategory)
            .ApplyFilters(queryParams)
            .GroupBy(e => e.TransactionCategory.Name)
            .Select(g => new TransactionTotalByCategoryDTO
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(e => e.Amount)
            })
            .OrderBy(g => g.TotalAmount)
            .ToListAsync();

        return totalTransactionByCategory;
    }

    public async Task<IEnumerable<TransactionTotalByTimeStringDTO>?> GetTotalTransactionByTimeAsync(
        string userID, 
        string timePeriod)
    {
        if (timePeriod != "day" && timePeriod != "week" &&
            timePeriod != "month" && timePeriod != "year")
        {
            return null;
        }

        var timePeriodValue = new NpgsqlParameter("timePeriodValue", timePeriod);
        var userIdValue = new NpgsqlParameter("userIdValue", userID);
        
        var totalTransactionByTime = await context.Database.SqlQueryRaw<TransactionTotalByTimeDTO>($"""
            SELECT date_trunc(@timePeriodValue, "TransactionDate") AS "TimePeriod", SUM("Amount") AS "TotalAmount" 
            FROM "{context.Set<T>().EntityType.GetTableName()}"
            WHERE "UserId" = @userIdValue
            GROUP BY "TimePeriod"
            ORDER BY "TimePeriod"
            """, timePeriodValue, userIdValue)
            .ToListAsync();

            var mapDateTimeToStringResult = totalTransactionByTime
                .Select(t => new TransactionTotalByTimeStringDTO
                {
                    TimePeriod = Utils.ConvertTimeValue(timePeriod, t.TimePeriod),
                    TotalAmount = t.TotalAmount,
                });

        return mapDateTimeToStringResult;
    }

    public async Task<TransactionGetDTO?> GetTransactionByIdAsync(string id)
    {
        var transaction = await context.Set<T>()
            .Include(e => e.TransactionCategory)
            .Include(e => e.Currency)
            .Where(e => e.Id == id)
            .Select(e => new TransactionGetDTO
            {
                Description = e.Description,
                CategoryName = e.TransactionCategory.Name,
                Amount = e.Amount,
                CurrencyName = e.Currency.Code,
                TransactionDate = e.TransactionDate
            })
            .FirstOrDefaultAsync();
        
        return transaction;
    }

    public async Task<IEnumerable<TransactionGetDTO>> GetTransactionsAsync(
        [FromQuery] TransactionQueryParams? queryParams)
    {
        var transactions = await context.Set<T>()
            .Include(e => e.TransactionCategory)
            .Include(e => e.Currency)
            .ApplyFilters(queryParams)
            .Select(e => new TransactionGetDTO
            {
                Description = e.Description,
                CategoryName = e.TransactionCategory.Name,
                Amount = e.Amount,
                CurrencyName = e.Currency.Code,
                TransactionDate = e.TransactionDate
            })
            .ToListAsync();

        return transactions;
    }

    public async Task<int> UpdateTransactionAsync(
        string id, 
        TransactionCreateDTO transactionUpdateDTO)
    {
        var item = await context.Set<T>().FindAsync(id);
        if (item == null) return 0;

        item.Description = transactionUpdateDTO.Description;
        item.TransactionCategoryId = transactionUpdateDTO.CategoryId;
        item.Amount = transactionUpdateDTO.Amount;
        item.CurrencyId = transactionUpdateDTO.CurrencyId;
        item.TransactionDate = transactionUpdateDTO.TransactionDate;
        
        context.Set<T>().Update(item);
        return await context.SaveChangesAsync();
    }
}