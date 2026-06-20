using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        
        builder.Property(c => c.Code)
            .HasMaxLength(3)
            .IsRequired();
        
        builder.HasKey(c => c.Id);

        builder.HasData(
            new Currency { Id = 1, Code = "USD" },
            new Currency { Id = 2, Code = "EUR" },
            new Currency { Id = 3, Code = "GBP" },
            new Currency { Id = 4, Code = "JPY" },
            new Currency { Id = 5, Code = "AUD" },
            new Currency { Id = 6, Code = "VND" }
        );
    }
}
