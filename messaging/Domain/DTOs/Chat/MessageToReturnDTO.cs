using System;

namespace messaging.Domain.DTOs.Chat;

public class MessageToReturnDTO
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }

    public Guid? ReceiverId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public string? ChatRoomName { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}
