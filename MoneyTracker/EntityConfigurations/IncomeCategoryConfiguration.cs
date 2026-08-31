using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class IncomeCategoryConfiguration : IEntityTypeConfiguration<IncomeCategory>
{
    public void Configure(EntityTypeBuilder<IncomeCategory> builder)
    {
        builder.Property(ic => ic.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(ic => ic.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasKey(ic => ic.Id);

        builder.HasData(
            new IncomeCategory { Id = 1, Name = "Salary" },
            new IncomeCategory { Id = 2, Name = "Business" },
            new IncomeCategory { Id = 3, Name = "Investments" },
            new IncomeCategory { Id = 4, Name = "Gifts" },
            new IncomeCategory { Id = 5, Name = "Other" }
        );
    }
}
