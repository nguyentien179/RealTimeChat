using System;
using messaging.Application.Common;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;

namespace messaging.Application.Interfaces;

public interface IChatRoomService
{
    Task<ChatRoomToReturnDTO> CreateChatRoomAsync(ChatRoomToAddDTO dto);
    Task<PagedResponse<ChatRoomToReturnDTO>> GetChatRoomByIdAsync(
        Guid chatRoomId,
        int pageNumber,
        int pageSize
    );
    Task AddUsersToChatRoomAsync(AddUserToRoomDTO dto);
    Task KickUserFromRoomAsync(Guid chatRoomId, Guid userIdToKick);
    Task<ChatRoomToReturnDTO> UpdateChatRoomAsync(ChatRoomToUpdateDTO dto);
    Task LeaveChatRoomAsync(Guid chatRoomId, Guid userId);
}
