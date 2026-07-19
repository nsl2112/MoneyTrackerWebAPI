using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController([FromKeyedServices("Expense")] ITransaction expenseService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionGetDTO>>> GetExpenses([FromQuery] TransactionQueryParams? queryParams)
        {
            var expenses = await expenseService.GetTransactionsAsync(queryParams);
            return Ok(expenses);
        } 

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionGetDTO>> GetExpense(string id)
        {     
            var expense = await expenseService.GetTransactionByIdAsync(id);
            if (expense == null) 
            {
                return NotFound();          
            }
            
            return Ok(expense);
        }

        [HttpGet("total")]
        public async Task<ActionResult<TransactionTotalByCategoryDTO>> GetTotalExpenses([FromQuery] TransactionQueryParams? queryParams)
        {
            var totalExpenses = await expenseService.GetTotalTransactionAsync(queryParams);
            return Ok(totalExpenses);
        }

        [HttpGet("total-by-category")]
        public async Task<ActionResult<IEnumerable<TransactionTotalByCategoryDTO>>> GetTotalExpensesByCategory([FromQuery] TransactionQueryParams? queryParams)
        {
            var totalExpensesByCategory = await expenseService.GetTotalTransactionByCategoryAsync(queryParams);
            return Ok(totalExpensesByCategory);
        }

        [HttpGet("total-by-time")]
        public async Task<ActionResult<IEnumerable<TransactionTotalByTimeStringDTO>>> GetTotalExpensesByTime(string timePeriod)
        {
            var userID = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var totalExpensesByTime = await expenseService.GetTotalTransactionByTimeAsync(userID, timePeriod);

            if (totalExpensesByTime == null)
            {
                return BadRequest("Invalid time period. Please use 'day', 'week', 'month', or 'year'.");  
            }

            return Ok(totalExpensesByTime);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense(TransactionCreateDTO expenseDTO)
        {
            var userID = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var expense = await expenseService.CreateTransactionAsync(userID, expenseDTO);
            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expenseDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(string? id, TransactionCreateDTO expense)
        {
            var updatedCount = await expenseService.UpdateTransactionAsync(id, expense);
            if (updatedCount == 0) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(string? id)
        {
            var deletedItemsCount = await expenseService.DeleteTransactionAsync(id);       
            if (deletedItemsCount == 0) return NotFound();
            return NoContent();
        }
    }
}