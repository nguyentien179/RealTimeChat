using System;
using messaging.Application.Common;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using messaging.Domain.Entity;

namespace messaging.Application.Interfaces;

public interface IChatService
{
    Task<MessageToReturnDTO> SaveMessageAsync(MessageToSendDTO message);
    Task<PagedResponse<MessageToReturnDTO>> GetPrivateMessagesAsync(
        Guid user1,
        Guid user2,
        int page,
        int pageSize
    );
    Task<PagedResponse<Guid>> GetChatPartnersAsync(Guid userId, int page, int pageSize);
    Task<PagedResponse<ChatRoomToReturnDTO>> GetUserChatRoomsAsync(
        Guid userId,
        int page,
        int pageSize
    );
}
