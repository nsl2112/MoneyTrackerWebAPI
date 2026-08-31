using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyTracker;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole(Roles.Admin)
            {
                Id = "d3b29c0e-c962-4325-af66-d0dc00168245",
                NormalizedName = Roles.Admin.ToUpper()
            },
            new IdentityRole(Roles.User)
            {
                Id = "e07d669a-576c-4e94-a329-598bdd9ec066",
                NormalizedName = Roles.User.ToUpper()
            }
        );
    }
}
