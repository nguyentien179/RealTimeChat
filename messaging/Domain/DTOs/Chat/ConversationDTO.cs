using System;
using messaging.Domain.Enum;

namespace messaging.Domain.DTOs.Chat;

public class ConversationDTO
{
    public ConversationType Type { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
