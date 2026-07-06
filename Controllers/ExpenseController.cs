using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker
{
    [Authorize]
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
                .ApplyFilters(queryParams)
                .Select(e => new ExpenseGetDTO
                {
                    Description = e.Description,
                    ExpenseCategoryName = e.ExpenseCategory.Name,
                    Amount = e.Amount,
                    CurrencyName = e.Currency.Code,
                    TransactionDate = e.TransactionDate
                });

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

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalExpenses([FromQuery] ExpenseQuerryParams? queryParams)
        {
            var expenses = context.ExpenseItems
                .AsQueryable()
                .ApplyFilters(queryParams);

            var totalExpenses = await expenses.SumAsync(e => e.Amount);
            return Ok(totalExpenses);
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverageExpenses([FromQuery] ExpenseQuerryParams? queryParams)
        {
            var expenses = context.ExpenseItems
                .AsQueryable()
                .ApplyFilters(queryParams);

            var averageExpenses = await expenses.AverageAsync(e => e.Amount);
            return Ok(averageExpenses);
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
            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expenseDTO);
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
            
            return NoContent();
        }
    }
}
