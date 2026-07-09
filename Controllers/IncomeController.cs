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
        public async Task<IActionResult> GetIncomes(IncomeQuerryParams? queryParams)
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
        public async Task<IActionResult> GetTotalIncome(IncomeQuerryParams? queryParams)
        {
            var total = await context.IncomeItems
                .ApplyFilters(queryParams)
                .SumAsync(i => i.Amount);
            return Ok(total);
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverageIncome(IncomeQuerryParams? queryParams)
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
