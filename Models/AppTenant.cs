using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MoneyTracker;

public class AppTenant
{
    public string Id {get; set;} = Guid.NewGuid().ToString();
    public string Name {get; set;} = null!;
    public ICollection<AppUser> Users {get; set;} = new List<AppUser>();
}
