namespace MoneyTracker;

public interface IExpenseCategoryService
{
    Task<IEnumerable<ExpenseCategoryDTO>> GetAllAsync();
    Task<ExpenseCategoryDTO?> GetByIdAsync(int id);
    Task<ExpenseCategoryDTO> CreateAsync(CreateExpenseCategoryDTO dto);
    Task<ExpenseCategoryDTO?> UpdateAsync(int id, UpdateExpenseCategoryDTO dto);
    Task<bool> DeleteAsync(int id);
}
