using System;

namespace messaging.Domain.DTOs.ChatRoom;

public class ChatRoomToAddDTO
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> UserIds { get; set; } = new();
}
