using messaging.Application.Interfaces;
using messaging.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace messaging.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid userId1, Guid userId2)
    {
        return await _context
            .ChatMessages.Where(m =>
                (m.SenderId == userId1 && m.ReceiverId == userId2)
                || (m.SenderId == userId2 && m.ReceiverId == userId1)
            )
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetGroupMessagesAsync(string chatRoom)
    {
        return await _context
            .ChatMessages.Where(m => m.ChatRoom == chatRoom)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
}
