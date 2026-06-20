using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        
        builder.Property(b => b.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasKey(b => b.Id);

        builder.HasData(
            new ExpenseCategory { Id = 1, Name = "Food" },
            new ExpenseCategory { Id = 2, Name = "Transportation" },
            new ExpenseCategory { Id = 3, Name = "Entertainment" },
            new ExpenseCategory { Id = 4, Name = "Utilities" },
            new ExpenseCategory { Id = 5, Name = "Healthcare" },
            new ExpenseCategory { Id = 6, Name = "Education" },
            new ExpenseCategory { Id = 7, Name = "Personal Care" },
            new ExpenseCategory { Id = 8, Name = "Clothing" },
            new ExpenseCategory { Id = 9, Name = "Gifts" },
            new ExpenseCategory { Id = 10, Name = "Other" }
        );
    }
}
