using System;
using FluentValidation;
using messaging.Application.Interfaces;
using messaging.Application.Mappers;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.Entity;
using messaging.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace messaging.Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IValidator<MessageToSendDTO> _messageValidator;

    public ChatService(
        IChatRepository chatRepository,
        IHubContext<ChatHub> hubContext,
        IValidator<MessageToSendDTO> messageValidator
    )
    {
        _chatRepository = chatRepository;
        _hubContext = hubContext;
        _messageValidator = messageValidator;
    }

    public async Task<MessageToReturnDTO> SaveMessageAsync(MessageToSendDTO message)
    {
        var result = await _messageValidator.ValidateAsync(message);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
        var messageToSend = message.ToEntity();
        await _chatRepository.SaveMessageAsync(messageToSend);
        if (!string.IsNullOrEmpty(message.ChatRoom))
        {
            await _hubContext
                .Clients.Group(message.ChatRoom)
                .SendAsync("ReceiveGroupMessage", message);
        }
        else if (message.ReceiverId.HasValue)
        {
            await _hubContext
                .Clients.User(message.ReceiverId.ToString()!)
                .SendAsync("ReceiveMessage", message);
        }
        return messageToSend.ToReturnDTO();
    }

    public async Task<IEnumerable<MessageToReturnDTO>> GetPrivateMessagesAsync(
        Guid user1,
        Guid user2
    )
    {
        var messages = await _chatRepository.GetMessagesAsync(user1, user2);
        return messages.Select(ChatMapper.ToReturnDTO).ToList();
    }

    public async Task<IEnumerable<MessageToReturnDTO>> GetGroupMessagesAsync(string chatRoom)
    {
        var messages = await _chatRepository.GetGroupMessagesAsync(chatRoom);
        return messages.Select(ChatMapper.ToReturnDTO).ToList();
    }
}
