using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoneyTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedingExpenseItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExpenseItems",
                columns: new[] { "Id", "Amount", "CurrencyId", "Description", "ExpenseCategoryId", "TransactionDate", "UserId" },
                values: new object[,]
                {
                    { "df3afc2e-dff2-4b79-bdc2-7d96c08643bb", 2.75m, 1, "Bus Ticket", 2, new DateTime(2026, 6, 19, 12, 36, 10, 0, DateTimeKind.Utc), null },
                    { "f16819d9-9cbc-4eed-93f1-74afee8055b4", 15.50m, 1, "Lunch at Cafe", 1, new DateTime(2026, 6, 20, 12, 30, 0, 0, DateTimeKind.Utc), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExpenseItems",
                keyColumn: "Id",
                keyValue: "df3afc2e-dff2-4b79-bdc2-7d96c08643bb");

            migrationBuilder.DeleteData(
                table: "ExpenseItems",
                keyColumn: "Id",
                keyValue: "f16819d9-9cbc-4eed-93f1-74afee8055b4");
        }
    }
}
