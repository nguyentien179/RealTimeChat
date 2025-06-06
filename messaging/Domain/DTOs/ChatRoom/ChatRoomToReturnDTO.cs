using System;
using messaging.Application.Common;
using messaging.Domain.DTOs.Chat;

namespace messaging.Domain.DTOs.ChatRoom;

public class ChatRoomToReturnDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> UserIds { get; set; } = new List<Guid>();
    public PagedResponse<MessageToReturnDTO> Messages { get; set; } = new PagedResponse<MessageToReturnDTO>();
}
