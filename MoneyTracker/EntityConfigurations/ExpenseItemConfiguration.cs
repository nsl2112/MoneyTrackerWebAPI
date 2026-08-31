using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class ExpenseItemTypeConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {      
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        builder.Property(e => e.TransactionCategoryId)
            .HasColumnName("ExpenseCategoryId");
        
        builder.HasOne(e => e.TransactionCategory)
            .WithMany()
            .HasForeignKey(e => e.TransactionCategoryId)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.Property(e => e.TransactionDate)
            .HasColumnType("TIMESTAMP")
            .IsRequired();
        
        builder.HasData(
            new ExpenseItem
            {
                Id = "f16819d9-9cbc-4eed-93f1-74afee8055b4",
                Description = "Lunch at Cafe",
                TransactionCategoryId = 1,
                Amount = 15.50m,
                CurrencyId = 1,
                TransactionDate = new DateTime(2026, 06, 20, 12, 30, 0),
                UserId = null
            },
            new ExpenseItem
            {
                Id = "df3afc2e-dff2-4b79-bdc2-7d96c08643bb",
                Description = "Bus Ticket",
                TransactionCategoryId = 2,
                Amount = 2.75m,
                CurrencyId = 1,
                TransactionDate = new DateTime(2026, 06, 19, 12, 36, 10),
                UserId = null
            }
        );
    }
}
