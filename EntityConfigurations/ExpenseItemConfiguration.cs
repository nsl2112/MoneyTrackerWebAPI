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
        
        builder.HasOne<ExpenseCategory>(e => e.ExpenseCategory)
            .WithMany()
            .HasForeignKey(e => e.ExpenseCategoryId)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne<Currency>(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.Property(e => e.TransactionDate)
            .HasColumnType("TIMESTAMPTZ")
            .IsRequired();

        builder.HasOne<AppUser>(e => e.AppUser)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
