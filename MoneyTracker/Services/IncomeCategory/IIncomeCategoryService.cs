namespace MoneyTracker;

public interface IIncomeCategoryService
{
    Task<IEnumerable<IncomeCategoryDTO>> GetAllAsync();
    Task<IncomeCategoryDTO?> GetByIdAsync(int id);
    Task<IncomeCategoryDTO> CreateAsync(CreateIncomeCategoryDTO dto);
    Task<IncomeCategoryDTO?> UpdateAsync(int id, UpdateIncomeCategoryDTO dto);
    Task<bool> DeleteAsync(int id);
}
