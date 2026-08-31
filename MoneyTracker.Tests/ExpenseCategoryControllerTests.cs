using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MoneyTracker.Tests;

public class ExpenseCategoryControllerTests
{
    [Fact]
    public async Task GetExpenseCategories_ReturnsOkWithCategories()
    {
        var service = new Mock<IExpenseCategoryService>();
        service
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ExpenseCategoryDTO>
            {
                new() { Id = 1, Name = "Food" },
                new() { Id = 2, Name = "Transport" }
            });

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.GetExpenseCategories();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<ExpenseCategoryDTO>>(okResult.Value);
        Assert.Equal(2, categories.Count());
    }

    [Fact]
    public async Task GetExpenseCategoryById_WhenFound_ReturnsOk()
    {
        var service = new Mock<IExpenseCategoryService>();
        service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ExpenseCategoryDTO { Id = 1, Name = "Food" });

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.GetExpenseCategoryById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var category = Assert.IsType<ExpenseCategoryDTO>(okResult.Value);
        Assert.Equal("Food", category.Name);
    }

    [Fact]
    public async Task GetExpenseCategoryById_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IExpenseCategoryService>();
        service.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ExpenseCategoryDTO?)null);

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.GetExpenseCategoryById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateExpenseCategory_WhenValid_ReturnsCreatedResponse()
    {
        var service = new Mock<IExpenseCategoryService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateExpenseCategoryDTO>()))
            .ReturnsAsync(new ExpenseCategoryDTO { Id = 4, Name = "Bills" });

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.CreateExpenseCategory(new CreateExpenseCategoryDTO { Name = "Bills" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ExpenseCategoryController.GetExpenseCategoryById), created.ActionName);
        Assert.Equal(4, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task CreateExpenseCategory_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var service = new Mock<IExpenseCategoryService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateExpenseCategoryDTO>()))
            .ThrowsAsync(new ArgumentException("Category name is invalid."));

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.CreateExpenseCategory(new CreateExpenseCategoryDTO { Name = "" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Category name is invalid.", badRequest.Value);
    }

    [Fact]
    public async Task CreateExpenseCategory_WhenServiceThrowsInvalidOperationException_ReturnsConflict()
    {
        var service = new Mock<IExpenseCategoryService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateExpenseCategoryDTO>()))
            .ThrowsAsync(new InvalidOperationException("Category already exists."));

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.CreateExpenseCategory(new CreateExpenseCategoryDTO { Name = "Food" });

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("Category already exists.", conflict.Value);
    }

    [Fact]
    public async Task UpdateExpenseCategory_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<IExpenseCategoryService>();
        service
            .Setup(s => s.UpdateAsync(2, It.IsAny<UpdateExpenseCategoryDTO>()))
            .ReturnsAsync(new ExpenseCategoryDTO { Id = 2, Name = "Travel" });

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.UpdateExpenseCategory(2, new UpdateExpenseCategoryDTO { Name = "Travel" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateExpenseCategory_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IExpenseCategoryService>();
        service.Setup(s => s.UpdateAsync(99, It.IsAny<UpdateExpenseCategoryDTO>())).ReturnsAsync((ExpenseCategoryDTO?)null);

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.UpdateExpenseCategory(99, new UpdateExpenseCategoryDTO { Name = "Other" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteExpenseCategory_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<IExpenseCategoryService>();
        service.Setup(s => s.DeleteAsync(4)).ReturnsAsync(true);

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.DeleteExpenseCategory(4);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteExpenseCategory_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IExpenseCategoryService>();
        service.Setup(s => s.DeleteAsync(40)).ReturnsAsync(false);

        var controller = new ExpenseCategoryController(service.Object);

        var result = await controller.DeleteExpenseCategory(40);

        Assert.IsType<NotFoundResult>(result);
    }
}
