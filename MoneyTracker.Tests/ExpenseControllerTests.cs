using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Moq;

namespace MoneyTracker.Tests;

public class ExpenseControllerTests
{
    private static ExpenseController CreateController(ITransaction service)
    {
        var controller = new ExpenseController(service);
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
    public async Task GetExpenses_ReturnsOkWithExpenses()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTransactionsAsync(It.IsAny<TransactionQueryParams?>()))
            .ReturnsAsync(new List<TransactionGetDTO>
            {
                new() { Description = "Groceries", CategoryName = "Food", Amount = 42.75m, CurrencyName = "USD", TransactionDate = DateTime.UtcNow },
                new() { Description = "Fuel", CategoryName = "Transport", Amount = 60m, CurrencyName = "USD", TransactionDate = DateTime.UtcNow }
            });

        var controller = CreateController(service.Object);

        var result = await controller.GetExpenses(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var expenses = Assert.IsAssignableFrom<IEnumerable<TransactionGetDTO>>(okResult.Value);
        Assert.Equal(2, expenses.Count());
    }

    [Fact]
    public async Task GetExpense_WhenFound_ReturnsOk()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.GetTransactionByIdAsync("exp-1")).ReturnsAsync(new TransactionGetDTO
        {
            Description = "Groceries",
            CategoryName = "Food",
            Amount = 42.75m,
            CurrencyName = "USD",
            TransactionDate = DateTime.UtcNow
        });

        var controller = CreateController(service.Object);

        var result = await controller.GetExpense("exp-1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var expense = Assert.IsType<TransactionGetDTO>(okResult.Value);
        Assert.Equal("Groceries", expense.Description);
    }

    [Fact]
    public async Task GetExpense_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.GetTransactionByIdAsync("missing")).ReturnsAsync((TransactionGetDTO?)null);

        var controller = CreateController(service.Object);

        var result = await controller.GetExpense("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetTotalExpenses_ReturnsOkWithTotal()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTotalTransactionAsync(It.IsAny<TransactionQueryParams?>()))
            .ReturnsAsync(new TransactionTotalByCategoryDTO { CategoryName = "Food", TotalAmount = 120m });

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalExpenses(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var total = Assert.IsType<TransactionTotalByCategoryDTO>(okResult.Value);
        Assert.Equal(120m, total.TotalAmount);
    }

    [Fact]
    public async Task GetTotalExpensesByCategory_ReturnsOkWithTotals()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTotalTransactionByCategoryAsync(It.IsAny<TransactionQueryParams?>()))
            .ReturnsAsync(new List<TransactionTotalByCategoryDTO>
            {
                new() { CategoryName = "Food", TotalAmount = 90m },
                new() { CategoryName = "Transport", TotalAmount = 120m }
            });

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalExpensesByCategory(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var totals = Assert.IsAssignableFrom<IEnumerable<TransactionTotalByCategoryDTO>>(okResult.Value);
        Assert.Equal(2, totals.Count());
    }

    [Fact]
    public async Task GetTotalExpensesByTime_WhenPeriodIsValid_ReturnsOk()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.GetTotalTransactionByTimeAsync("user-123", "month"))
            .ReturnsAsync(new List<TransactionTotalByTimeStringDTO>
            {
                new() { TimePeriod = "2026-08", TotalAmount = 250m }
            });

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalExpensesByTime("month");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var totals = Assert.IsAssignableFrom<IEnumerable<TransactionTotalByTimeStringDTO>>(okResult.Value);
        Assert.Single(totals);
    }

    [Fact]
    public async Task GetTotalExpensesByTime_WhenPeriodIsInvalid_ReturnsBadRequest()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.GetTotalTransactionByTimeAsync("user-123", "bad-period")).ReturnsAsync((IEnumerable<TransactionTotalByTimeStringDTO>?)null);

        var controller = CreateController(service.Object);

        var result = await controller.GetTotalExpensesByTime("bad-period");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid time period. Please use 'day', 'week', 'month', or 'year'.", badRequest.Value);
    }

    [Fact]
    public async Task CreateExpense_WhenValid_ReturnsCreatedResponse()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.CreateTransactionAsync("user-123", It.IsAny<TransactionCreateDTO>()))
            .ReturnsAsync(new ExpenseItem { Id = "exp-2", Description = "Office supplies" });

        var controller = CreateController(service.Object);

        var dto = new TransactionCreateDTO
        {
            Description = "Office supplies",
            CategoryId = 2,
            Amount = 75m,
            CurrencyId = 1,
            TransactionDate = DateTime.UtcNow
        };

        var result = await controller.CreateExpense(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ExpenseController.GetExpense), created.ActionName);
        Assert.Equal("exp-2", created.RouteValues!["id"]);
    }

    [Fact]
    public async Task UpdateExpense_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.UpdateTransactionAsync("exp-1", It.IsAny<TransactionCreateDTO>())).ReturnsAsync(1);

        var controller = CreateController(service.Object);

        var result = await controller.UpdateExpense("exp-1", new TransactionCreateDTO
        {
            Description = "Updated expense",
            CategoryId = 3,
            Amount = 90m,
            CurrencyId = 1,
            TransactionDate = DateTime.UtcNow
        });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateExpense_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.UpdateTransactionAsync("missing", It.IsAny<TransactionCreateDTO>())).ReturnsAsync(0);

        var controller = CreateController(service.Object);

        var result = await controller.UpdateExpense("missing", new TransactionCreateDTO
        {
            Description = "Missing expense",
            CategoryId = 1,
            Amount = 12m,
            CurrencyId = 1,
            TransactionDate = DateTime.UtcNow
        });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteExpense_WhenFound_ReturnsNoContent()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.DeleteTransactionAsync("exp-1")).ReturnsAsync(1);

        var controller = CreateController(service.Object);

        var result = await controller.DeleteExpense("exp-1");

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteExpense_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ITransaction>();
        service.Setup(s => s.DeleteTransactionAsync("missing")).ReturnsAsync(0);

        var controller = CreateController(service.Object);

        var result = await controller.DeleteExpense("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}
