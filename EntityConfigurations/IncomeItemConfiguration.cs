using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class IncomeItemConfiguration : IEntityTypeConfiguration<IncomeItem>
{
    public void Configure(EntityTypeBuilder<IncomeItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description)
            .HasMaxLength(200);

        builder.Property(i => i.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TransactionDate)
            .HasConversion(v => v.ToUniversalTime(), 
                           v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            .HasColumnType("TIMESTAMPTZ")
            .IsRequired();

        builder.HasOne(i => i.IncomeCategory)
            .WithMany()
            .HasForeignKey(i => i.IncomeCategoryId)
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
    }
}
