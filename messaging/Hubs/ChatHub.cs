using messaging.Application.Interfaces;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace messaging.Hubs;

[Authorize]
public class ChatHub(IChatService chatService, IChatRoomService chatRoomService) : Hub
{
    private readonly IChatService _chatService = chatService;
    private readonly IChatRoomService _chatRoomService = chatRoomService;

    // Save to DB and broadcast to receiver
    public async Task SendMessage(MessageToSendDTO message)
    {
        await _chatService.SaveMessageAsync(message);
    }

    public async Task AddToRoom(AddUserToRoomDTO addUserToRoomDTO)
    {
        await _chatRoomService.AddUsersToChatRoomAsync(addUserToRoomDTO);
    }

    public async Task LeaveRoom(Guid roomId, Guid userId)
    {
        await _chatRoomService.LeaveChatRoomAsync(roomId, userId);
    }

    public async Task KickUserFromRoom(Guid roomId, Guid userIdToKick)
    {
        await _chatRoomService.KickUserFromRoomAsync(roomId, userIdToKick);
    }
}
