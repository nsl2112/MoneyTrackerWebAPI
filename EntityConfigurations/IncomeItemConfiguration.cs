using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class IncomeItemConfiguration : IEntityTypeConfiguration<IncomeItem>
{
    private readonly AppDbContext context = null!;
    public void Configure(EntityTypeBuilder<IncomeItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description)
            .HasMaxLength(200);

        builder.Property(i => i.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TransactionDate)
            .HasColumnType("TIMESTAMP")
            .IsRequired();

        builder.Property(i => i.TransactionCategoryId)
            .HasColumnName("IncomeCategoryId");

        builder.HasOne(i => i.TransactionCategory)
            .WithMany()
            .HasForeignKey(i => i.TransactionCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(i => i.Currency)
            .WithMany()
            .HasForeignKey(i => i.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(i => i.AppUser)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(i => i.UserId == context.TentantId);
    }
}
