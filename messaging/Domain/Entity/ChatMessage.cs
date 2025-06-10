using System;

namespace messaging.Domain.Entity;

public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid SenderId { get; set; } // Just a GUID
    public Guid? ReceiverId { get; set; } // For 1-on-1 chats

    public Guid? ChatRoomId { get; set; }
    public ChatRoom? ChatRoom { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; } = true;
}
