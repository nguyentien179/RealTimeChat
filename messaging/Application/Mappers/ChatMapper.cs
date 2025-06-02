using System;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.Entity;

namespace messaging.Application.Mappers;

public static class ChatMapper
{
    public static ChatMessage ToEntity(this MessageToSendDTO dto)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = dto.SenderId,
            ReceiverId = dto.ReceiverId,
            ChatRoom = dto.ChatRoom,
            Content = dto.Content,
            Timestamp = DateTime.UtcNow
        };
    }

    public static MessageToReturnDTO ToReturnDTO(this ChatMessage message)
    {
        return new MessageToReturnDTO
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            ChatRoom = message.ChatRoom,
            Content = message.Content,
            Timestamp = message.Timestamp
        };
    }
}
