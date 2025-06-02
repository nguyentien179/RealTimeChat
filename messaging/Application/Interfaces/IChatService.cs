using System;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.Entity;

namespace messaging.Application.Interfaces;

public interface IChatService
{
    Task<MessageToReturnDTO> SaveMessageAsync(MessageToSendDTO message);
    Task<IEnumerable<MessageToReturnDTO>> GetPrivateMessagesAsync(Guid user1, Guid user2);
    Task<IEnumerable<MessageToReturnDTO>> GetGroupMessagesAsync(string chatRoom);
}
