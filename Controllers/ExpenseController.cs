using System.Globalization;
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
            var expenses = await context.ExpenseItems
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

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalExpenses([FromQuery] ExpenseQuerryParams? queryParams)
        {
            var totalExpenses = context.ExpenseItems
                .ApplyFilters(queryParams)
                .SumAsync(e => e.Amount);

            return Ok(totalExpenses);
        }

        [HttpGet("total-by-category")]
        public async Task<IActionResult> GetTotalExpensesByCategory([FromQuery] ExpenseQuerryParams? queryParams)
        {
            var totalExpensesByCategory = await context.ExpenseItems
                .Include(e => e.ExpenseCategory)
                .ApplyFilters(queryParams)
                .GroupBy(e => e.ExpenseCategory.Name)
                .Select(g => new
                {
                    ExpenseCategoryName = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .OrderBy(g => g.TotalAmount)
                .ToListAsync();

            return Ok(totalExpensesByCategory);
        }

        [HttpGet("total-by-time")]
        public async Task<IActionResult> GetTotalExpensesByTime(string timePeriod)
        {
            var totalExpensesByTime = context.Database
                .SqlQuery<ExpenseTotalByTimeDTO>($"""
                    SELECT date_trunc({timePeriod}, "TransactionDate") AS "TimePeriod", SUM("Amount") AS "TotalAmount" 
                    FROM "ExpenseItems"
                    GROUP BY "TimePeriod"
                    ORDER BY "TimePeriod"
                    """);
                
            if (timePeriod == "day")
            {
                var totalExpensesByDay = await totalExpensesByTime
                    .Select(e => new
                    {
                        Day = e.TimePeriod.ToString("yyyy-MM-dd"),
                        TotalAmount = e.TotalAmount
                    })
                    .ToListAsync();

                return Ok(totalExpensesByDay);    
            }
            else if (timePeriod == "week")
            {
                var totalExpensesByWeek = await totalExpensesByTime
                    .Select(e => new
                    {
                        Week = $"{e.TimePeriod.Year}-W{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(e.TimePeriod, CalendarWeekRule.FirstDay, DayOfWeek.Monday)}",
                        TotalAmount = e.TotalAmount
                    })
                    .ToListAsync();
                return Ok(totalExpensesByWeek);
            }
            else if (timePeriod == "month")
            {
                var totalExpensesByMonth = await totalExpensesByTime
                    .Select(e => new
                    {
                        Month = new DateTime(e.TimePeriod.Year, e.TimePeriod.Month, 1).ToString("yyyy-MM"),
                        TotalAmount = e.TotalAmount
                    })
                    .ToListAsync();
                return Ok(totalExpensesByMonth);
            }
            else if (timePeriod == "year")
            {
                var totalExpensesByYear = await totalExpensesByTime
                    .Select(e => new
                    {
                        Year = new DateTime(e.TimePeriod.Year, 1, 1).ToString("yyyy"),
                        TotalAmount = e.TotalAmount
                    })
                    .ToListAsync();
                return Ok(totalExpensesByYear);
            }

            return BadRequest("Invalid time period. Please use 'day', 'week', 'month', or 'year'.");
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
                TransactionDate = expenseDTO.TransactionDate,
                UserId = User.FindFirst("sub")?.Value
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
