using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MoneyTracker.Tests;

public class IncomeCategoryControllerTests
{
    [Fact]
    public async Task GetIncomeCategories_ReturnsOkWithCategories()
    {
        var service = new Mock<IIncomeCategoryService>();
        service
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<IncomeCategoryDTO>
            {
                new() { Id = 1, Name = "Salary" },
                new() { Id = 2, Name = "Freelance" }
            });

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.GetIncomeCategories();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<IncomeCategoryDTO>>(okResult.Value);
        Assert.Equal(2, categories.Count());
    }

    [Fact]
    public async Task GetIncomeCategoryById_WhenFound_ReturnsOk()
    {
        var service = new Mock<IIncomeCategoryService>();
        service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new IncomeCategoryDTO { Id = 1, Name = "Salary" });

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.GetIncomeCategoryById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var category = Assert.IsType<IncomeCategoryDTO>(okResult.Value);
        Assert.Equal("Salary", category.Name);
    }

    [Fact]
    public async Task GetIncomeCategoryById_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IIncomeCategoryService>();
        service.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((IncomeCategoryDTO?)null);

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.GetIncomeCategoryById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateIncomeCategory_WhenValid_ReturnsCreatedResponse()
    {
        var service = new Mock<IIncomeCategoryService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateIncomeCategoryDTO>()))
            .ReturnsAsync(new IncomeCategoryDTO { Id = 6, Name = "Bonus" });

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.CreateIncomeCategory(new CreateIncomeCategoryDTO { Name = "Bonus" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(IncomeCategoryController.GetIncomeCategoryById), created.ActionName);
        Assert.Equal(6, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task CreateIncomeCategory_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var service = new Mock<IIncomeCategoryService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateIncomeCategoryDTO>()))
            .ThrowsAsync(new ArgumentException("Category name is invalid."));

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.CreateIncomeCategory(new CreateIncomeCategoryDTO { Name = "" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Category name is invalid.", badRequest.Value);
    }

    [Fact]
    public async Task CreateIncomeCategory_WhenServiceThrowsInvalidOperationException_ReturnsConflict()
    {
        var service = new Mock<IIncomeCategoryService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateIncomeCategoryDTO>()))
            .ThrowsAsync(new InvalidOperationException("Category already exists."));

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.CreateIncomeCategory(new CreateIncomeCategoryDTO { Name = "Salary" });

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("Category already exists.", conflict.Value);
    }

    [Fact]
    public async Task UpdateIncomeCategory_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<IIncomeCategoryService>();
        service
            .Setup(s => s.UpdateAsync(2, It.IsAny<UpdateIncomeCategoryDTO>()))
            .ReturnsAsync(new IncomeCategoryDTO { Id = 2, Name = "Consulting" });

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.UpdateIncomeCategory(2, new UpdateIncomeCategoryDTO { Name = "Consulting" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateIncomeCategory_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IIncomeCategoryService>();
        service.Setup(s => s.UpdateAsync(99, It.IsAny<UpdateIncomeCategoryDTO>())).ReturnsAsync((IncomeCategoryDTO?)null);

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.UpdateIncomeCategory(99, new UpdateIncomeCategoryDTO { Name = "Other" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteIncomeCategory_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<IIncomeCategoryService>();
        service.Setup(s => s.DeleteAsync(5)).ReturnsAsync(true);

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.DeleteIncomeCategory(5);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteIncomeCategory_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IIncomeCategoryService>();
        service.Setup(s => s.DeleteAsync(40)).ReturnsAsync(false);

        var controller = new IncomeCategoryController(service.Object);

        var result = await controller.DeleteIncomeCategory(40);

        Assert.IsType<NotFoundResult>(result);
    }
}
