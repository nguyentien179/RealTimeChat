using System;

namespace messaging.Domain.Entity;

public class ChatRoomUser
{
    public Guid ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = default!;

    public Guid UserId { get; set; }
}
