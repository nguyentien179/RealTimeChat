using System;

namespace messaging.Domain.DTOs.Chat;

public class MessageToSendDTO
{
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public string? ChatRoom { get; set; }
    public string Content { get; set; } = string.Empty;
}
