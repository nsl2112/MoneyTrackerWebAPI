using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class TenantConfiguration : IEntityTypeConfiguration<AppTenant>
{
    public void Configure(EntityTypeBuilder<AppTenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(t => t.Users)
            .WithMany(u => u.Tenants)
            .UsingEntity( j => j.ToTable("AppTenantUser"));
    }
}
