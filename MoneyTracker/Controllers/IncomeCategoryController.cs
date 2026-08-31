using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker;

[Authorize]
[Route("api/income-categories")]
[ApiController]
public class IncomeCategoryController(IIncomeCategoryService incomeCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncomeCategoryDTO>>> GetIncomeCategories()
    {
        var categories = await incomeCategoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncomeCategoryDTO>> GetIncomeCategoryById(int id)
    {
        var category = await incomeCategoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<IncomeCategoryDTO>> CreateIncomeCategory(CreateIncomeCategoryDTO dto)
    {
        try
        {
            var category = await incomeCategoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetIncomeCategoryById), new { id = category.Id }, category);
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
    public async Task<IActionResult> UpdateIncomeCategory(int id, UpdateIncomeCategoryDTO dto)
    {
        try
        {
            var updated = await incomeCategoryService.UpdateAsync(id, dto);
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
    public async Task<IActionResult> DeleteIncomeCategory(int id)
    {
        var deleted = await incomeCategoryService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
