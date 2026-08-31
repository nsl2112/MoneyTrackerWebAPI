using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker;

[Authorize]
[Route("api/expense-categories")]
[ApiController]
public class ExpenseCategoryController(IExpenseCategoryService expenseCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseCategoryDTO>>> GetExpenseCategories()
    {
        var categories = await expenseCategoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseCategoryDTO>> GetExpenseCategoryById(int id)
    {
        var category = await expenseCategoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryDTO>> CreateExpenseCategory(CreateExpenseCategoryDTO dto)
    {
        try
        {
            var category = await expenseCategoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetExpenseCategoryById), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateExpenseCategory(int id, UpdateExpenseCategoryDTO dto)
    {
        try
        {
            var updated = await expenseCategoryService.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpenseCategory(int id)
    {
        var deleted = await expenseCategoryService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
