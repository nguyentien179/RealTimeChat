using System;

namespace messaging.Domain.DTOs.ChatRoom;

public class ChatRoomToUpdateDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid>? UserIds { get; set; }
}
