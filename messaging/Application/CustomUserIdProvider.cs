using System;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace messaging.Application;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var userId =
            connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? connection.User?.FindFirst("UserId")?.Value;

        Console.WriteLine($"[CustomUserIdProvider] Extracted UserId: {userId}");

        return userId;
    }
}
