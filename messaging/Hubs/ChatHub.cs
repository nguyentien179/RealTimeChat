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

    public async Task SendMessageToUser(string receiverUserId, MessageToReturnDTO message)
    {
        await Clients.User(receiverUserId).SendAsync("ReceiveMessage", message);
    }

    public async Task SendMessageToGroup(string chatRoom, MessageToReturnDTO message)
    {
        await Clients.Group(chatRoom).SendAsync("ReceiveGroupMessage", message);
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
        // Optional: log or manage connection
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Optional: cleanup
        return base.OnDisconnectedAsync(exception);
    }
}
