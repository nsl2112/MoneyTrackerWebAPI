namespace MoneyTracker;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDTO>> GetAllAsync();
    Task<CurrencyDTO?> GetByIdAsync(int id);
    Task<CurrencyDTO> CreateAsync(CreateCurrencyDTO dto);
    Task<CurrencyDTO?> UpdateAsync(int id, UpdateCurrencyDTO dto);
    Task<bool> DeleteAsync(int id);
}
