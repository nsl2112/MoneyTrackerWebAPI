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

            if (queryParams != null && !string.IsNullOrEmpty(queryParams.ExpenseCategory))
            {
                expenses = expenses.Where(e => e.ExpenseCategoryName == queryParams.ExpenseCategory);
            }

            if (queryParams != null && !string.IsNullOrEmpty(queryParams.Amount))
            {
                var values = queryParams.Amount.Split('-');
                if (values.Length == 1)
                {
                    if (decimal.TryParse(queryParams.Amount, out decimal amount))
                    {
                        expenses = expenses.Where(e => e.Amount == amount);
                    }
                }
                else if (values.Length == 2)
                {
                    if (decimal.TryParse(values[0], out decimal minAmount) && 
                        decimal.TryParse(values[1], out decimal maxAmount))
                    {
                        expenses = expenses.Where(e => e.Amount >= minAmount && e.Amount <= maxAmount);
                    }
                }
                else
                {
                    return BadRequest("Invalid amount range format. Use 'min-max' or a single value.");
                }
            }

            if (queryParams != null && !string.IsNullOrEmpty(queryParams.Date))
            {
                var values = queryParams.Date.Split('_');
                if (values.Length == 1)
                {
                    if (DateTime.TryParse(queryParams.Date, out DateTime date))
                    {
                        expenses = expenses.Where(e => e.TransactionDate.Date == date.Date);
                    }
                }
                else if (values.Length == 2)
                {
                    if (DateTime.TryParse(values[0], out DateTime startDate) && 
                        DateTime.TryParse(values[1], out DateTime endDate))
                    {
                        expenses = expenses.Where(e => e.TransactionDate.Date >= startDate.Date && e.TransactionDate.Date <= endDate.Date);
                    }
                }
                else
                {
                    return BadRequest("Invalid date range format. Use 'start-end' or a single date.");
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
