using System;

namespace messaging.Domain.DTOs.ChatRoom;

public class AddUserToRoomDTO
{
    public Guid ChatRoomId { get; set; }
    public List<Guid> UserIds { get; set; } = new();
}
