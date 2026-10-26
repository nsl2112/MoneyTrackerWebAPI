using System;

namespace MoneyTracker;

public interface ITokenService
{
    public (string token, DateTime expiration) GenerateToken(AppUser user, IEnumerable<string> roles);
}
