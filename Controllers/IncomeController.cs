using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController([FromKeyedServices("Income")] ITransaction incomeService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionGetDTO>>> GetIncomes([FromQuery] TransactionQueryParams? queryParams)
        {
            var incomes = await incomeService.GetTransactionsAsync(queryParams);
            return Ok(incomes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionGetDTO>> GetIncome(string id)
        {
            var income = await incomeService.GetTransactionByIdAsync(id);
            if (income == null)
            {
                return NotFound();
            }

            return Ok(income);
        }

        [HttpGet("total")]
        public async Task<ActionResult<TransactionTotalByCategoryDTO>> GetTotalIncome([FromQuery] TransactionQueryParams? queryParams)
        {
            var total = await incomeService.GetTotalTransactionAsync(queryParams);
            return Ok(total);
        }

        [HttpGet("total-by-category")]
        public async Task<ActionResult<IEnumerable<TransactionTotalByCategoryDTO>>> GetTotalIncomeByCategory([FromQuery] TransactionQueryParams? queryParams)
        {
            var totalByCategory = await incomeService.GetTotalTransactionByCategoryAsync(queryParams);
            return Ok(totalByCategory);
        }

        [HttpGet("total-by-time")]
        public async Task<ActionResult<IEnumerable<TransactionTotalByTimeStringDTO>>> GetTotalIncomesByTime([FromQuery] string timePeriod)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var totalIncomesByTime = await incomeService.GetTotalTransactionByTimeAsync(userId, timePeriod);

            if (totalIncomesByTime == null)
            {
                return BadRequest("Invalid time period. Please use 'day', 'week', 'month', or 'year'.");
            }

            return Ok(totalIncomesByTime);
        }

        [HttpPost]
        public async Task<ActionResult> CreateIncome(TransactionCreateDTO incomeDTO)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var income = await incomeService.CreateTransactionAsync(userId, incomeDTO);
            return CreatedAtAction(nameof(GetIncome), new { id = income.Id }, incomeDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIncome(string id, TransactionCreateDTO income)
        {
            var updatedCount = await incomeService.UpdateTransactionAsync(id, income);
            if (updatedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIncome(string id)
        {
            var deletedCount = await incomeService.DeleteTransactionAsync(id);
            if (deletedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
