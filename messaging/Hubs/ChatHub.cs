using System;
using messaging.Application.Interfaces;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.Entity;
using messaging.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace messaging.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

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

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
