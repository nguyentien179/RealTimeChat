using System;

namespace messaging.Domain.Entity;

public class ChatRoom
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ChatRoomUser> Users { get; set; } = new List<ChatRoomUser>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
