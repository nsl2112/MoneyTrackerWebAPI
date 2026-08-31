using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MoneyTracker.Tests;

public class IncomeControllerTests
{
    private static IncomeController CreateController(ITransaction service)
    {
        var controller = new IncomeController(service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(JwtRegisteredClaimNames.Sub, "user-123") },
                    authenticationType: "TestAuth"))
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetIncomes_ReturnsOkWithIncomes()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTransactionsAsync(It.IsAny<TransactionQueryParams?>()))
            .ReturnsAsync(new List<TransactionGetDTO>
            {
                new() { Description = "Salary", CategoryName = "Work", Amount = 4000m, CurrencyName = "USD", TransactionDate = DateTime.UtcNow },
                new() { Description = "Bonus", CategoryName = "Work", Amount = 500m, CurrencyName = "USD", TransactionDate = DateTime.UtcNow }
            });

        var controller = CreateController(service.Object);

        var result = await controller.GetIncomes(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var incomes = Assert.IsAssignableFrom<IEnumerable<TransactionGetDTO>>(okResult.Value);
        Assert.Equal(2, incomes.Count());
    }

    [Fact]
    public async Task GetIncome_WhenFound_ReturnsOk()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.GetTransactionByIdAsync("inc-1")).ReturnsAsync(new TransactionGetDTO
        {
            Description = "Salary",
            CategoryName = "Work",
            Amount = 4000m,
            CurrencyName = "USD",
            TransactionDate = DateTime.UtcNow
        });

        var controller = CreateController(service.Object);

        var result = await controller.GetIncome("inc-1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var income = Assert.IsType<TransactionGetDTO>(okResult.Value);
        Assert.Equal("Salary", income.Description);
    }

    [Fact]
    public async Task GetIncome_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.GetTransactionByIdAsync("missing")).ReturnsAsync((TransactionGetDTO?)null);

        var controller = CreateController(service.Object);

        var result = await controller.GetIncome("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetTotalIncome_ReturnsOkWithTotal()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTotalTransactionAsync(It.IsAny<TransactionQueryParams?>()))
            .ReturnsAsync(new TransactionTotalByCategoryDTO { CategoryName = "Work", TotalAmount = 4500m });

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalIncome(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var total = Assert.IsType<TransactionTotalByCategoryDTO>(okResult.Value);
        Assert.Equal(4500m, total.TotalAmount);
    }

    [Fact]
    public async Task GetTotalIncomeByCategory_ReturnsOkWithTotals()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTotalTransactionByCategoryAsync(It.IsAny<TransactionQueryParams?>()))
            .ReturnsAsync(new List<TransactionTotalByCategoryDTO>
            {
                new() { CategoryName = "Work", TotalAmount = 4300m },
                new() { CategoryName = "Investments", TotalAmount = 200m }
            });

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalIncomeByCategory(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var totals = Assert.IsAssignableFrom<IEnumerable<TransactionTotalByCategoryDTO>>(okResult.Value);
        Assert.Equal(2, totals.Count());
    }

    [Fact]
    public async Task GetTotalIncomesByTime_WhenPeriodIsValid_ReturnsOk()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTotalTransactionByTimeAsync("user-123", "month"))
            .ReturnsAsync(new List<TransactionTotalByTimeStringDTO>
            {
                new() { TimePeriod = "2026-08", TotalAmount = 4500m }
            });

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalIncomesByTime("month");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var totals = Assert.IsAssignableFrom<IEnumerable<TransactionTotalByTimeStringDTO>>(okResult.Value);
        Assert.Single(totals);
    }

    [Fact]
    public async Task GetTotalIncomesByTime_WhenPeriodIsInvalid_ReturnsBadRequest()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.GetTotalTransactionByTimeAsync("user-123", "bad-period")).ReturnsAsync((IEnumerable<TransactionTotalByTimeStringDTO>?)null);

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalIncomesByTime("bad-period");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid time period. Please use 'day', 'week', 'month', or 'year'.", badRequest.Value);
    }

    [Fact]
    public async Task CreateIncome_WhenValid_ReturnsCreatedResponse()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.CreateTransactionAsync("user-123", It.IsAny<TransactionCreateDTO>()))
            .ReturnsAsync(new IncomeItem { Id = "inc-2", Description = "Side gig" });

        var controller = CreateController(service.Object);

        var dto = new TransactionCreateDTO
        {
            Description = "Side gig",
            CategoryId = 2,
            Amount = 900m,
            CurrencyId = 1,
            TransactionDate = DateTime.UtcNow
        };

        var result = await controller.CreateIncome(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(IncomeController.GetIncome), created.ActionName);
        Assert.Equal("inc-2", created.RouteValues!["id"]);
    }

    [Fact]
    public async Task UpdateIncome_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.UpdateTransactionAsync("inc-1", It.IsAny<TransactionCreateDTO>())).ReturnsAsync(1);

        var controller = CreateController(service.Object);

        var result = await controller.UpdateIncome("inc-1", new TransactionCreateDTO
        {
            Description = "Updated salary",
            CategoryId = 1,
            Amount = 4200m,
            CurrencyId = 1,
            TransactionDate = DateTime.UtcNow
        });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateIncome_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.UpdateTransactionAsync("missing", It.IsAny<TransactionCreateDTO>())).ReturnsAsync(0);

        var controller = CreateController(service.Object);

        var result = await controller.UpdateIncome("missing", new TransactionCreateDTO
        {
            Description = "Unknown income",
            CategoryId = 1,
            Amount = 100m,
            CurrencyId = 1,
            TransactionDate = DateTime.UtcNow
        });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteIncome_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.DeleteTransactionAsync("inc-1")).ReturnsAsync(1);

        var controller = CreateController(service.Object);

        var result = await controller.DeleteIncome("inc-1");

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteIncome_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.DeleteTransactionAsync("missing")).ReturnsAsync(0);

        var controller = CreateController(service.Object);

        var result = await controller.DeleteIncome("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}
