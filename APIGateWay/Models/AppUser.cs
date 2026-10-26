using System;
using Microsoft.AspNetCore.Identity;

namespace MoneyTracker;

public class AppUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public ICollection<AppTenant> Tenants { get; set; } = new List<AppTenant>();
}
