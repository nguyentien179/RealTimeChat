using System;
using messaging.Domain.Entity;

namespace messaging.Application.Interfaces;

public interface IChatRepository
{
    Task SaveMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<ChatMessage>> GetGroupMessagesAsync(string chatRoom);
    Task<IEnumerable<Guid>> GetChatPartnersAsync(Guid userId);
    Task<IEnumerable<string>> GetUserChatRoomsAsync(Guid userId);
}
