using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MoneyTracker.Tests;

public class CurrencyControllerTests
{
    [Fact]
    public async Task GetCurrencies_ReturnsOkWithCurrencies()
    {
        var service = new Mock<ICurrencyService>();
        service
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<CurrencyDTO>
            {
                new() { Id = 1, Code = "USD" },
                new() { Id = 2, Code = "EUR" }
            });

        var controller = new CurrencyController(service.Object);

        var result = await controller.GetCurrencies();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var currencies = Assert.IsAssignableFrom<IEnumerable<CurrencyDTO>>(okResult.Value);
        Assert.Equal(2, currencies.Count());
    }

    [Fact]
    public async Task GetCurrency_WhenFound_ReturnsOk()
    {
        var service = new Mock<ICurrencyService>();
        service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new CurrencyDTO { Id = 1, Code = "USD" });

        var controller = new CurrencyController(service.Object);

        var result = await controller.GetCurrency(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var currency = Assert.IsType<CurrencyDTO>(okResult.Value);
        Assert.Equal("USD", currency.Code);
    }

    [Fact]
    public async Task GetCurrency_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICurrencyService>();
        service.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((CurrencyDTO?)null);

        var controller = new CurrencyController(service.Object);

        var result = await controller.GetCurrency(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateCurrency_WhenValid_ReturnsCreatedResponse()
    {
        var service = new Mock<ICurrencyService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateCurrencyDTO>()))
            .ReturnsAsync(new CurrencyDTO { Id = 7, Code = "JPY" });

        var controller = new CurrencyController(service.Object);

        var result = await controller.CreateCurrency(new CreateCurrencyDTO { Code = "JPY" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(CurrencyController.GetCurrency), created.ActionName);
        Assert.Equal(7, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task CreateCurrency_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var service = new Mock<ICurrencyService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateCurrencyDTO>()))
            .ThrowsAsync(new ArgumentException("Invalid currency code."));

        var controller = new CurrencyController(service.Object);

        var result = await controller.CreateCurrency(new CreateCurrencyDTO { Code = "BAD" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid currency code.", badRequest.Value);
    }

    [Fact]
    public async Task CreateCurrency_WhenServiceThrowsInvalidOperationException_ReturnsConflict()
    {
        var service = new Mock<ICurrencyService>();
        service
            .Setup(s => s.CreateAsync(It.IsAny<CreateCurrencyDTO>()))
            .ThrowsAsync(new InvalidOperationException("Currency already exists."));

        var controller = new CurrencyController(service.Object);

        var result = await controller.CreateCurrency(new CreateCurrencyDTO { Code = "USD" });

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("Currency already exists.", conflict.Value);
    }

    [Fact]
    public async Task UpdateCurrency_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<ICurrencyService>();
        service
            .Setup(s => s.UpdateAsync(3, It.IsAny<UpdateCurrencyDTO>()))
            .ReturnsAsync(new CurrencyDTO { Id = 3, Code = "GBP" });

        var controller = new CurrencyController(service.Object);

        var result = await controller.UpdateCurrency(3, new UpdateCurrencyDTO { Code = "GBP" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateCurrency_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICurrencyService>();
        service.Setup(s => s.UpdateAsync(99, It.IsAny<UpdateCurrencyDTO>())).ReturnsAsync((CurrencyDTO?)null);

        var controller = new CurrencyController(service.Object);

        var result = await controller.UpdateCurrency(99, new UpdateCurrencyDTO { Code = "CHF" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCurrency_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<ICurrencyService>();
        service.Setup(s => s.DeleteAsync(5)).ReturnsAsync(true);

        var controller = new CurrencyController(service.Object);

        var result = await controller.DeleteCurrency(5);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteCurrency_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICurrencyService>();
        service.Setup(s => s.DeleteAsync(40)).ReturnsAsync(false);

        var controller = new CurrencyController(service.Object);

        var result = await controller.DeleteCurrency(40);

        Assert.IsType<NotFoundResult>(result);
    }
}
