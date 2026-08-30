using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTracker;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CurrencyController(ICurrencyService currencyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrencyDTO>>> GetCurrencies()
    {
        var currencies = await currencyService.GetAllAsync();
        return Ok(currencies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CurrencyDTO>> GetCurrency(int id)
    {
        var currency = await currencyService.GetByIdAsync(id);
        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }

    [HttpPost]
    public async Task<ActionResult<CurrencyDTO>> CreateCurrency(CreateCurrencyDTO dto)
    {
        try
        {
            var currency = await currencyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetCurrency), new { id = currency.Id }, currency);
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
    public async Task<IActionResult> UpdateCurrency(int id, UpdateCurrencyDTO dto)
    {
        try
        {
            var updated = await currencyService.UpdateAsync(id, dto);
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
    public async Task<IActionResult> DeleteCurrency(int id)
    {
        var deleted = await currencyService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
