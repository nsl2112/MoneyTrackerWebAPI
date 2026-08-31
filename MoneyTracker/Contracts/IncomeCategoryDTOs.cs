namespace MoneyTracker;

public class IncomeCategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateIncomeCategoryDTO
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateIncomeCategoryDTO
{
    public string Name { get; set; } = string.Empty;
}
