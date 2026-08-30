using Microsoft.EntityFrameworkCore;

namespace MoneyTracker;

public class ExpenseCategoryService(TenantDbContext context) : IExpenseCategoryService
{
    public async Task<IEnumerable<ExpenseCategoryDTO>> GetAllAsync()
    {
        return await context.ExpenseCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ExpenseCategoryDTO { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<ExpenseCategoryDTO?> GetByIdAsync(int id)
    {
        return await context.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ExpenseCategoryDTO { Id = c.Id, Name = c.Name })
            .FirstOrDefaultAsync();
    }

    public async Task<ExpenseCategoryDTO> CreateAsync(CreateExpenseCategoryDTO dto)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(dto));
        }

        var exists = await context.ExpenseCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists)
        {
            throw new InvalidOperationException("An expense category with this name already exists.");
        }

        var category = new ExpenseCategory { Name = name };
        context.ExpenseCategories.Add(category);
        await context.SaveChangesAsync();

        return new ExpenseCategoryDTO { Id = category.Id, Name = category.Name };
    }

    public async Task<ExpenseCategoryDTO?> UpdateAsync(int id, UpdateExpenseCategoryDTO dto)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(dto));
        }

        var category = await context.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return null;
        }

        var duplicateExists = await context.ExpenseCategories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            throw new InvalidOperationException("An expense category with this name already exists.");
        }

        category.Name = name;
        await context.SaveChangesAsync();

        return new ExpenseCategoryDTO { Id = category.Id, Name = category.Name };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await context.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return false;
        }

        var inUse = await context.ExpenseItems.AnyAsync(item => item.TransactionCategoryId == id);
        if (inUse)
        {
            return false;
        }

        context.ExpenseCategories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}
