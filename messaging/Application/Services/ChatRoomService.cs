using System;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using messaging.Application.Common;
using messaging.Application.Interfaces;
using messaging.Application.Mappers;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using messaging.Domain.Entity;
using messaging.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace messaging.Application.Services;

public class ChatRoomService : IChatRoomService
{
    private readonly IGenericRepository<ChatRoom> _chatRoomRepository;
    private readonly IGenericRepository<ChatMessage> _chatRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatRoomService(
        IGenericRepository<ChatRoom> chatRoomRepository,
        IGenericRepository<ChatMessage> chatRepository,
        IHubContext<ChatHub> hubContext
    )
    {
        _chatRoomRepository = chatRoomRepository;
        _chatRepository = chatRepository;
        _hubContext = hubContext;
    }

    public async Task<ChatRoomToReturnDTO> CreateChatRoomAsync(ChatRoomToAddDTO dto)
    {
        var room = dto.ToEntity();
        await _chatRoomRepository.AddAsync(room);
        await _chatRoomRepository.SaveChangesAsync();
        return room.ToDTO();
    }

    public async Task AddUsersToChatRoomAsync(AddUserToRoomDTO dto)
    {
        var chatRoom = await _chatRoomRepository.GetByIdAsync(dto.ChatRoomId);
        if (chatRoom == null)
        {
            throw new Exception("Chat room not found");
        }

        var existingUserIds = chatRoom.Users.Select(u => u.UserId).ToHashSet();
        foreach (var userId in dto.UserIds)
        {
            if (!existingUserIds.Contains(userId))
            {
                chatRoom.Users.Add(new ChatRoomUser { UserId = userId });
            }
        }

        await _chatRoomRepository.SaveChangesAsync();
        await _hubContext
            .Clients.Group(chatRoom.Id.ToString())
            .SendAsync(
                "UserAddedToRoom",
                new
                {
                    Message = "User has joined the room",
                    UserIds = dto.UserIds,
                    RoomId = chatRoom.Id
                }
            );
    }

    public async Task KickUserFromRoomAsync(Guid chatRoomId, Guid userIdToKick)
    {
        var room = await _chatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null)
            throw new Exception("Chat room not found");

        var user = room.Users.FirstOrDefault(u => u.UserId == userIdToKick);
        if (user != null)
        {
            room.Users.Remove(user);
            await _chatRoomRepository.SaveChangesAsync();
            await _hubContext
                .Clients.Group(chatRoomId.ToString())
                .SendAsync(
                    "UserLeftRoom",
                    new
                    {
                        UserId = userIdToKick,
                        RoomId = chatRoomId,
                        IsKicked = true,
                    }
                );
        }
    }

    public async Task LeaveChatRoomAsync(Guid chatRoomId, Guid userId)
    {
        var room = await _chatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null)
            throw new Exception("Chat room not found");

        var user = room.Users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            room.Users.Remove(user);
            await _chatRoomRepository.SaveChangesAsync();
            await _hubContext
                .Clients.Group(chatRoomId.ToString())
                .SendAsync(
                    "UserLeftRoom",
                    new
                    {
                        UserId = userId,
                        RoomId = chatRoomId,
                        IsKicked = false,
                    }
                );
        }
    }

    public async Task<ChatRoomToReturnDTO> UpdateChatRoomAsync(ChatRoomToUpdateDTO dto)
    {
        var chatRoom = await _chatRoomRepository.GetByIdAsync(dto.Id);
        if (chatRoom == null)
            throw new Exception("Chat room not found");

        // Update name
        chatRoom.UpdateFromDTO(dto);

        await _chatRoomRepository.SaveChangesAsync();
        await _hubContext
            .Clients.Group(chatRoom.Id.ToString())
            .SendAsync("ChatRoomUpdated", new { Message = "Chat room updated" });
        return chatRoom.ToDTO();
    }

    public async Task<ChatRoomToReturnDTO> GetChatRoomByIdAsync(
        Guid chatRoomId,
        int pageNumber,
        int pageSize
    )
    {
        var chatRoom =
            await _chatRoomRepository.GetByIdAsync(
                chatRoomId,
                c => c.Include(r => r.Users).Include(m => m.Messages)
            ) ?? throw new Exception("Chat room not found");

        var userIds = chatRoom.Users.Select(u => u.UserId).ToList();

        var totalMessages = chatRoom.Messages.Count;
        var pagedMessages = chatRoom
            .Messages.OrderByDescending(m => m.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => m.ToReturnDTO())
            .ToList();

        return new ChatRoomToReturnDTO
        {
            Id = chatRoom.Id,
            Name = chatRoom.Name,
            UserIds = userIds,
            Messages = new PagedResponse<MessageToReturnDTO>
            {
                Items = pagedMessages,
                PageIndex = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalMessages,
                TotalPages = (int)Math.Ceiling((double)totalMessages / pageSize),
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < (int)Math.Ceiling((double)totalMessages / pageSize)
            }
        };
    }

}
