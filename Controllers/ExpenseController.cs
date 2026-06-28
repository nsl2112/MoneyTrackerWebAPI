using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetExpenses([FromQuery] ExpenseQuerryParams? queryParams)
        {
            var expenses = context.ExpenseItems
                .Include(e => e.ExpenseCategory)
                .Include(e => e.Currency)
                .Select(e => new ExpenseGetDTO
                {
                    Description = e.Description,
                    ExpenseCategoryName = e.ExpenseCategory.Name,
                    Amount = e.Amount,
                    CurrencyName = e.Currency.Code,
                    TransactionDate = e.TransactionDate
                });

            if (queryParams != null)
            {
                if (!string.IsNullOrEmpty(queryParams.ExpenseCategory))
                {
                    expenses = expenses.Where(e => e.ExpenseCategoryName == queryParams.ExpenseCategory);
                }

                if (queryParams.Amount != null)
                {
                    expenses = expenses.Where(e => e.Amount >= queryParams.Amount.MinAmount &&
                                                   e.Amount <= queryParams.Amount.MaxAmount);
                }

                if (queryParams.Date != null)
                {
                    expenses = expenses.Where(e => e.TransactionDate >= queryParams.Date.StartDate &&
                                                   e.TransactionDate <= queryParams.Date.EndDate);
                }
            }

            await expenses.ToListAsync();
            return Ok(expenses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpense(string id)
        {
            var expense = await context.ExpenseItems
                .Include(e => e.ExpenseCategory)
                .Include(e => e.Currency)
                .Where(e => e.Id == id)
                .Select(e => new ExpenseGetDTO
                {
                    Description = e.Description,
                    ExpenseCategoryName = e.ExpenseCategory.Name,
                    Amount = e.Amount,
                    CurrencyName = e.Currency.Code,
                    TransactionDate = e.TransactionDate
                })
                .FirstOrDefaultAsync();
            
            if (expense == null)
            {
                return NotFound();
            }
            
            return Ok(expense);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense(ExpenseCreateDTO expenseDTO)
        {
            var expense = new ExpenseItem
            {
                Description = expenseDTO.Description,
                ExpenseCategoryId = expenseDTO.ExpenseCategoryId,
                Amount = expenseDTO.Amount,
                CurrencyId = expenseDTO.CurrencyId,
                TransactionDate = expenseDTO.TransactionDate
            };
            context.ExpenseItems.Add(expense);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(string? id, ExpenseCreateDTO expense)
        {
            var item = await context.ExpenseItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.Description = expense.Description;
            item.ExpenseCategoryId = expense.ExpenseCategoryId;
            item.Amount = expense.Amount;
            item.CurrencyId = expense.CurrencyId;
            item.TransactionDate = expense.TransactionDate;

            context.ExpenseItems.Update(item);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(string? id)
        {
            var deletedItemsCount = await context.ExpenseItems
                .Where(e => e.Id == id)
                .ExecuteDeleteAsync();
            
            if (deletedItemsCount == 0)
            {
                return NotFound();
            }

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
