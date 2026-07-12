using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetIncomes([FromQuery] IncomeQuerryParams? queryParams)
        {
            var incomes = await context.IncomeItems
                .Include(i => i.IncomeCategory)
                .Include(i => i.Currency)
                .ApplyFilters(queryParams)
                .Select(i => new IncomeGetDTO
                {
                    Description = i.Description,
                    IncomeCategoryName = i.IncomeCategory.Name,
                    Amount = i.Amount,
                    CurrencyName = i.Currency.Code,
                    TransactionDate = i.TransactionDate
                })
                .ToListAsync();
            return Ok(incomes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncome(string id)
        {
            var income = await context.IncomeItems
                .Where(i => i.Id == id)
                .Include(i => i.IncomeCategory)
                .Include(i => i.Currency)
                .Select(i => new IncomeGetDTO
                {
                    Description = i.Description,
                    IncomeCategoryName = i.IncomeCategory.Name,
                    Amount = i.Amount,
                    CurrencyName = i.Currency.Code,
                    TransactionDate = i.TransactionDate
                })
                .FirstOrDefaultAsync();

            if (income == null)
            {
                return NotFound();
            }
            return Ok(income);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalIncome([FromQuery] IncomeQuerryParams? queryParams)
        {
            var total = await context.IncomeItems
                .ApplyFilters(queryParams)
                .SumAsync(i => i.Amount);
            return Ok(new { Message = "Total income", TotalAmount = total });
        }

        [HttpGet("total-by-category")]
        public async Task<IActionResult> GetTotalIncomeByCategory([FromQuery] IncomeQuerryParams? queryParams)
        {
            var totalByCategory = await context.IncomeItems
                .Include(i => i.IncomeCategory)
                .ApplyFilters(queryParams)
                .GroupBy(i => i.IncomeCategory.Name)
                .Select(g => new
                {
                    IncomeCategory = g.Key,
                    TotalAmount = g.Sum(i => i.Amount)
                })
                .ToListAsync();

            return Ok(totalByCategory);
        }

        [HttpGet("total-by-time")]
        public async Task<IActionResult> GetTotalIncomesByTime([FromQuery] string timePeriod)
        {
            var totalIncomesByTime = context.Database
                .SqlQuery<IncomeTotalByTimeDTO>($"""
                    SELECT date_trunc({timePeriod}, "TransactionDate") AS "TimePeriod", SUM("Amount") AS "TotalAmount" 
                    FROM "IncomeItems"
                    WHERE "UserId" = {User.FindFirst("sub")?.Value}
                    GROUP BY "TimePeriod"
                    ORDER BY "TimePeriod"
                    """);
                
            if (timePeriod == "day")
            {
                var totalIncomesByDay = await totalIncomesByTime
                    .Select(i => new
                    {
                        Day = i.TimePeriod.ToString("yyyy-MM-dd"),
                        TotalAmount = i.TotalAmount
                    })
                    .ToListAsync();

                return Ok(totalIncomesByDay);    
            }
            else if (timePeriod == "week")
            {
                var totalIncomesByWeek = await totalIncomesByTime
                    .Select(i => new
                    {
                        Week = $"{i.TimePeriod.Year}-W{CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(i.TimePeriod, CalendarWeekRule.FirstDay, DayOfWeek.Monday)}",
                        TotalAmount = i.TotalAmount
                    })
                    .ToListAsync();
                return Ok(totalIncomesByWeek);
            }
            else if (timePeriod == "month")
            {
                var totalIncomesByMonth = await totalIncomesByTime
                    .Select(i => new
                    {
                        Month = new DateTime(i.TimePeriod.Year, i.TimePeriod.Month, 1).ToString("yyyy-MM"),
                        TotalAmount = i.TotalAmount
                    })
                    .ToListAsync();
                return Ok(totalIncomesByMonth);
            }
            else if (timePeriod == "year")
            {
                var totalIncomesByYear = await totalIncomesByTime
                    .Select(i => new
                    {
                        Year = new DateTime(i.TimePeriod.Year, 1, 1).ToString("yyyy"),
                        TotalAmount = i.TotalAmount
                    })
                    .ToListAsync();
                return Ok(totalIncomesByYear);
            }

            return BadRequest("Invalid time period. Please use 'day', 'week', 'month', or 'year'.");
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverageIncome([FromQuery] IncomeQuerryParams? queryParams)
        {
            var average = await context.IncomeItems
                .ApplyFilters(queryParams)
                .AverageAsync(i => i.Amount);
            return Ok(average);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIncome(IncomeCreateDTO incomeDTO)
        {
            var income = new IncomeItem
            {
                Description = incomeDTO.Description,
                IncomeCategoryId = incomeDTO.IncomeCategoryId,
                Amount = incomeDTO.Amount,
                CurrencyId = incomeDTO.CurrencyId,
                TransactionDate = incomeDTO.TransactionDate,
                UserId = User.FindFirst("sub")?.Value,
            };

            context.IncomeItems.Add(income);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIncome), new { id = income.Id }, incomeDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncome(string id, IncomeCreateDTO income)
        {
            var item = await context.IncomeItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.Description = income.Description;
            item.IncomeCategoryId = income.IncomeCategoryId;
            item.Amount = income.Amount;
            item.CurrencyId = income.CurrencyId;
            item.TransactionDate = income.TransactionDate;

            context.IncomeItems.Update(item);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncome(string id)
        {
            var deleted = await context.IncomeItems
                .Where(i => i.Id == id)
                .ExecuteDeleteAsync();
                
            if (deleted == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
