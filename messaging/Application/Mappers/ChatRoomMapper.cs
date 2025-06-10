using System;
using messaging.Application.Common;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using messaging.Domain.Entity;

namespace messaging.Application.Mappers;

public static class ChatRoomMapper
{
    public static ChatRoomToReturnDTO ToDTO(this ChatRoom room, PagingParameters? paging = null)
    {
        if (room == null)
        {
            return new ChatRoomToReturnDTO
            {
                Id = Guid.Empty,
                Name = string.Empty,
                UserIds = new List<Guid>(),
                Messages = new PagedResponse<MessageToReturnDTO>
                {
                    Items = new List<MessageToReturnDTO>(),
                    PageIndex = 1,
                    PageSize = paging?.PageSize ?? 20,
                    TotalRecords = 0,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false
                }
            };
        }

        paging ??= new PagingParameters { PageNumber = 1, PageSize = 20 };

        var messages = room.Messages?.OrderBy(m => m.Timestamp).ToList() ?? new List<ChatMessage>();
        var totalCount = messages.Count;

        var pagedMessages = messages
            .Skip((paging.PageNumber - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .Select(m => m.ToReturnDTO())
            .ToList();

        return new ChatRoomToReturnDTO
        {
            Id = room.Id,
            Name = room.Name ?? string.Empty,
            UserIds = room.Users?.Select(u => u.UserId).ToList() ?? new List<Guid>(),
            Messages = new PagedResponse<MessageToReturnDTO>
            {
                Items = pagedMessages,
                PageIndex = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalRecords = totalCount,
                TotalPages =
                    totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / paging.PageSize),
                HasPreviousPage = totalCount == 0 ? false : paging.PageNumber > 1,
                HasNextPage =
                    totalCount == 0
                        ? false
                        : paging.PageNumber
                            < (int)Math.Ceiling((double)totalCount / paging.PageSize)
            }
        };
    }

    public class PagingParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public static ChatRoom ToEntity(this ChatRoomToAddDTO dto) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Users = dto.UserIds.Select(id => new ChatRoomUser { UserId = id }).ToList()
        };

    public static void UpdateFromDTO(this ChatRoom chatRoom, ChatRoomToUpdateDTO dto)
    {
        chatRoom.Name = dto.Name;
    }
}
