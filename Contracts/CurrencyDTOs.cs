namespace MoneyTracker;

public class CurrencyDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class CreateCurrencyDTO
{
    public string Code { get; set; } = string.Empty;
}

public class UpdateCurrencyDTO
{
    public string Code { get; set; } = string.Empty;
}
