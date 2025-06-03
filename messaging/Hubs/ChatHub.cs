using messaging.Application.Interfaces;
using messaging.Domain.DTOs.Chat;
using Microsoft.AspNetCore.SignalR;

namespace messaging.Hubs;

public class ChatHub(IChatService chatService) : Hub
{
    private readonly IChatService _chatService = chatService;

    // Save to DB and broadcast to receiver
    public async Task SendMessageToUser(string receiverUserId, MessageToSendDTO message)
    {
        // Receiver is set via parameter, assign it to the message
        message.ReceiverId = Guid.Parse(receiverUserId);
        await _chatService.SaveMessageAsync(message);
    }

    // Save to DB and broadcast to group
    public async Task SendMessageToGroup(string chatRoom, MessageToSendDTO message)
    {
        message.ChatRoom = chatRoom;
        await _chatService.SaveMessageAsync(message);
    }

    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
    }

    public async Task LeaveRoom(string room)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
    }
}
