using Microsoft.EntityFrameworkCore;

namespace MoneyTracker;

public class CurrencyService(TenantDbContext context) : ICurrencyService
{
    public async Task<IEnumerable<CurrencyDTO>> GetAllAsync()
    {
        return await context.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDTO { Id = c.Id, Code = c.Code })
            .ToListAsync();
    }

    public async Task<CurrencyDTO?> GetByIdAsync(int id)
    {
        return await context.Currencies
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CurrencyDTO { Id = c.Id, Code = c.Code })
            .FirstOrDefaultAsync();
    }

    public async Task<CurrencyDTO> CreateAsync(CreateCurrencyDTO dto)
    {
        var code = dto.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code is required.", nameof(dto));
        }

        var exists = await context.Currencies
            .AnyAsync(c => c.Code.ToLower() == code.ToLower());

        if (exists)
        {
            throw new InvalidOperationException("A currency with this code already exists.");
        }

        var currency = new Currency { Code = code };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        return new CurrencyDTO { Id = currency.Id, Code = currency.Code };
    }

    public async Task<CurrencyDTO?> UpdateAsync(int id, UpdateCurrencyDTO dto)
    {
        var code = dto.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code is required.", nameof(dto));
        }

        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == id);
        if (currency == null)
        {
            return null;
        }

        var duplicateExists = await context.Currencies
            .AnyAsync(c => c.Id != id && c.Code.ToLower() == code.ToLower());

        if (duplicateExists)
        {
            throw new InvalidOperationException("A currency with this code already exists.");
        }

        currency.Code = code;
        await context.SaveChangesAsync();

        return new CurrencyDTO { Id = currency.Id, Code = currency.Code };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == id);
        if (currency == null)
        {
            return false;
        }

        var inUseByExpense = await context.ExpenseItems.AnyAsync(item => item.CurrencyId == id);
        var inUseByIncome = await context.IncomeItems.AnyAsync(item => item.CurrencyId == id);

        if (inUseByExpense || inUseByIncome)
        {
            return false;
        }

        context.Currencies.Remove(currency);
        await context.SaveChangesAsync();
        return true;
    }
}
