namespace MoneyTracker;

public class ExpenseCategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateExpenseCategoryDTO
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateExpenseCategoryDTO
{
    public string Name { get; set; } = string.Empty;
}
