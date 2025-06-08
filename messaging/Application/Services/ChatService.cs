using System;
using System.Linq.Expressions;
using FluentValidation;
using messaging.Application.Common;
using messaging.Application.Interfaces;
using messaging.Application.Mappers;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using messaging.Domain.Entity;
using messaging.Domain.Enum;
using messaging.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace messaging.Application.Services;

public class ChatService : IChatService
{
    private readonly IGenericRepository<ChatMessage> _chatRepository;
    private readonly IGenericRepository<ChatRoom> _chatRoomRepository;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IValidator<MessageToSendDTO> _messageValidator;

    public ChatService(
        IGenericRepository<ChatMessage> chatRepository,
        IHubContext<ChatHub> hubContext,
        IValidator<MessageToSendDTO> messageValidator,
        IGenericRepository<ChatRoom> chatRoomRepository
    )
    {
        _chatRepository = chatRepository;
        _hubContext = hubContext;
        _messageValidator = messageValidator;
        _chatRoomRepository = chatRoomRepository;
    }

    public async Task<MessageToReturnDTO> SaveMessageAsync(MessageToSendDTO message)
    {
        var result = await _messageValidator.ValidateAsync(message);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var entity = message.ToEntity();
        await _chatRepository.AddAsync(entity);
        await _chatRepository.SaveChangesAsync();

        if (message.ChatRoomId.HasValue)
        {
            // Group message
            await _hubContext
                .Clients.Group(message.ChatRoomId.Value.ToString())
                .SendAsync("ReceiveMessage", entity.ToReturnDTO());
        }
        else if (message.ReceiverId.HasValue)
        {
            // Private message
            await _hubContext
                .Clients.User(message.ReceiverId.Value.ToString())
                .SendAsync("ReceiveMessage", entity.ToReturnDTO());
        }

        return entity.ToReturnDTO();
    }

    public Task<PagedResponse<MessageToReturnDTO>> GetPrivateMessagesAsync(
        Guid user1,
        Guid user2,
        int page,
        int pageSize
    )
    {
        var filters = new List<Expression<Func<ChatMessage, bool>>>
        {
            m =>
                (m.SenderId == user1 && m.ReceiverId == user2)
                || (m.SenderId == user2 && m.ReceiverId == user1)
        };

        return CreatePagedMessageResponse(filters, page, pageSize);
    }

    public async Task<PagedResponse<Guid>> GetChatPartnersAsync(Guid userId, int page, int pageSize)
    {
        var filters = new List<Expression<Func<ChatMessage, bool>>>
        {
            m => (m.SenderId == userId && m.ReceiverId != null) || m.ReceiverId == userId
        };

        var pagedResult = await _chatRepository.GetAllAsync(
            page,
            pageSize,
            filters,
            m => m.OrderByDescending(m => m.Timestamp)
        );

        var partnerIds = pagedResult
            .Items.Select(m => m.SenderId == userId ? m.ReceiverId!.Value : m.SenderId)
            .Distinct()
            .ToList();

        return new PagedResponse<Guid>
        {
            Items = partnerIds,
            PageIndex = pagedResult.PageIndex,
            PageSize = pagedResult.PageSize,
            TotalRecords = partnerIds.Count,
            TotalPages = (int)Math.Ceiling(partnerIds.Count / (double)pagedResult.PageSize),
            HasNextPage = pagedResult.HasNextPage,
            HasPreviousPage = pagedResult.HasPreviousPage
        };
    }

    public async Task<PagedResponse<ChatRoomToReturnDTO>> GetUserChatRoomsAsync(
        Guid userId,
        int page,
        int pageSize
    )
    {
        var filters = new List<Expression<Func<ChatRoom, bool>>>
        {
            c => c.Users.Any(u => u.UserId == userId)
        };
        var includeProperties = new List<Expression<Func<ChatRoom, object>>>
        {
            room => room.Users,
            room => room.Messages
        };
        var pagedResult = await _chatRoomRepository.GetAllAsync(
            page,
            pageSize,
            filters,
            orderBy: cr => cr.OrderByDescending(cr => cr.Messages.Max(m => m.Timestamp)),
            c => c.Include(c => c.Users).Include(c => c.Messages)
        );

        // Filter out null ChatRooms and map to DTOs
        var items = pagedResult.Items.Select(room => room.ToDTO()).ToList();

        return new PagedResponse<ChatRoomToReturnDTO>
        {
            Items = items,
            PageIndex = page,
            PageSize = pageSize,
            TotalRecords = pagedResult.TotalRecords,
            TotalPages = pagedResult.TotalPages,
            HasNextPage = pagedResult.HasNextPage,
            HasPreviousPage = pagedResult.HasPreviousPage
        };
    }

    private async Task<PagedResponse<MessageToReturnDTO>> CreatePagedMessageResponse(
        List<Expression<Func<ChatMessage, bool>>> filters,
        int page,
        int pageSize
    )
    {
        var pagedResult = await _chatRepository.GetAllAsync(
            page,
            pageSize,
            filters,
            m => m.OrderByDescending(m => m.Timestamp)
        );

        return new PagedResponse<MessageToReturnDTO>
        {
            Items = pagedResult.Items.Select(m => m.ToReturnDTO()),
            PageIndex = pagedResult.PageIndex,
            PageSize = pagedResult.PageSize,
            TotalRecords = pagedResult.TotalRecords,
            TotalPages = pagedResult.TotalPages,
            HasNextPage = pagedResult.HasNextPage,
            HasPreviousPage = pagedResult.HasPreviousPage
        };
    }

    public async Task<PagedResponse<ConversationDTO>> GetUserConversationsAsync(
        Guid userId,
        int pageNumber,
        int pageSize
    )
    {
        var result = new List<ConversationDTO>();

        var partnerIds = await GetChatPartnersAsync(userId, pageNumber, pageSize);

        foreach (var partnerId in partnerIds.Items)
        {
            var privateMessages = await GetPrivateMessagesAsync(userId, partnerId, 1, 1);
            var lastMessage = privateMessages.Items.FirstOrDefault();
            result.Add(
                new ConversationDTO
                {
                    Type = ConversationType.Private,
                    Id = partnerId,
                    Name = string.Empty,
                    LastMessage = lastMessage?.Content ?? string.Empty,
                    Avatar = string.Empty,
                    Timestamp = lastMessage?.Timestamp ?? DateTime.MinValue,
                }
            );
        }

        var groupChats = await GetUserChatRoomsAsync(userId, pageNumber, pageSize);

        foreach (var chatRoom in groupChats.Items)
        {
            var lastMessage = chatRoom.Messages.Items.FirstOrDefault();
            result.Add(
                new ConversationDTO
                {
                    Type = ConversationType.Group,
                    Id = chatRoom.Id,
                    Name = chatRoom.Name,
                    LastMessage = lastMessage?.Content ?? string.Empty,
                    Avatar = string.Empty,
                    Timestamp = lastMessage?.Timestamp ?? DateTime.MinValue,
                }
            );
        }

        // Sort by Timestamp DESC
        result = result.OrderByDescending(c => c.Timestamp).ToList();
        Console.WriteLine(result.Count);

        // Pagination
        var totalCount = result.Count;
        var pagedItems = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResponse<ConversationDTO>
        {
            Items = pagedItems,
            PageIndex = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount
        };
    }
}
