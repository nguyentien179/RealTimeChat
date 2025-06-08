using System;

namespace messaging.Infrastructure.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = default!;
    public int ExpirationHours { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
