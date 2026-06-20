using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetExpenses()
        {
            var expenses = await context.ExpenseItems
                .Include(e => e.ExpenseCategory)
                .Include(e => e.Currency)
                .Select(e => new ExpenseGetDTO
                {
                    Description = e.Description,
                    ExpenseCategoryName = e.ExpenseCategory.Name,
                    Amount = e.Amount,
                    CurrencyName = e.Currency.Code,
                    TransactionDate = e.TransactionDate
                })
                .ToListAsync();
            
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
        public async Task<IActionResult> UpdateExpense(string? id, ExpenseItem expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            context.Entry(expense).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(string? id)
        {
            await context.ExpenseItems
                .Where(e => e.Id == id)
                .ExecuteDeleteAsync();
            
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
