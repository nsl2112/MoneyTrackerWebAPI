using System;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker;

public interface ITransaction
{
    Task<IEnumerable<TransactionGetDTO>> GetTransactionsAsync([FromQuery] TransactionQueryParams? queryParams);
    Task<TransactionGetDTO?> GetTransactionByIdAsync(string id);
    Task<TransactionTotalByCategoryDTO> GetTotalTransactionAsync([FromQuery] TransactionQueryParams? queryParams);
    Task<IEnumerable<TransactionTotalByCategoryDTO>> GetTotalTransactionByCategoryAsync([FromQuery] TransactionQueryParams? queryParams);
    Task<IEnumerable<TransactionTotalByTimeStringDTO>?> GetTotalTransactionByTimeAsync(string userID, string timePeriod);
    Task<TransactionItem> CreateTransactionAsync(string userID, TransactionCreateDTO transactionCreateDTO);
    Task<int> UpdateTransactionAsync(string id, TransactionCreateDTO transactionUpdateDTO);
    Task<int> DeleteTransactionAsync(string id);
}
