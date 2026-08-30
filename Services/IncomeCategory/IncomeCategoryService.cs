using Microsoft.EntityFrameworkCore;

namespace MoneyTracker;

public class IncomeCategoryService(TenantDbContext context) : IIncomeCategoryService
{
    public async Task<IEnumerable<IncomeCategoryDTO>> GetAllAsync()
    {
        return await context.IncomeCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new IncomeCategoryDTO { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<IncomeCategoryDTO?> GetByIdAsync(int id)
    {
        return await context.IncomeCategories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new IncomeCategoryDTO { Id = c.Id, Name = c.Name })
            .FirstOrDefaultAsync();
    }

    public async Task<IncomeCategoryDTO> CreateAsync(CreateIncomeCategoryDTO dto)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(dto));
        }

        var exists = await context.IncomeCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists)
        {
            throw new InvalidOperationException("An income category with this name already exists.");
        }

        var category = new IncomeCategory { Name = name };
        context.IncomeCategories.Add(category);
        await context.SaveChangesAsync();

        return new IncomeCategoryDTO { Id = category.Id, Name = category.Name };
    }

    public async Task<IncomeCategoryDTO?> UpdateAsync(int id, UpdateIncomeCategoryDTO dto)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(dto));
        }

        var category = await context.IncomeCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return null;
        }

        var duplicateExists = await context.IncomeCategories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            throw new InvalidOperationException("An income category with this name already exists.");
        }

        category.Name = name;
        await context.SaveChangesAsync();

        return new IncomeCategoryDTO { Id = category.Id, Name = category.Name };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await context.IncomeCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return false;
        }

        var inUse = await context.IncomeItems.AnyAsync(item => item.TransactionCategoryId == id);
        if (inUse)
        {
            return false;
        }

        context.IncomeCategories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}
