using System.Reflection;
using messaging.Application.Common;
using messaging.Application.Interfaces;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using Microsoft.AspNetCore.Mvc;

namespace messaging.Presentation.Controllers
{
    [Route("chat")]
    [ApiController]
    public class ChatController(IChatService chatService) : ControllerBase
    {
        private readonly IChatService _chatService = chatService;

        [HttpGet("private")]
        public async Task<ActionResult<PagedResponse<MessageToReturnDTO>>> GetPrivateMessages(
            [FromQuery] Guid user1,
            [FromQuery] Guid user2,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize
        )
        {
            var messages = await _chatService.GetPrivateMessagesAsync(
                user1,
                user2,
                page: pageIndex,
                pageSize: pageSize
            );
            return Ok(messages);
        }

        [HttpGet("partners/{userId:guid}")]
        public async Task<ActionResult<PagedResponse<Guid>>> GetChatPartners(
            Guid userId,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize
        )
        {
            var partners = await _chatService.GetChatPartnersAsync(
                userId,
                page: pageIndex,
                pageSize: pageSize
            );
            return Ok(partners);
        }

        [HttpGet("groups/{userId:guid}")]
        public async Task<ActionResult<PagedResponse<ChatRoomToReturnDTO>>> GetUserGroups(
            Guid userId,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize
        )
        {
            var groups = await _chatService.GetUserChatRoomsAsync(
                userId,
                page: pageIndex,
                pageSize: pageSize
            );
            return Ok(groups);
        }

        [HttpGet("conversations/{userId:guid}")]
        public async Task<ActionResult<PagedResponse<ConversationDTO>>> GetConversations(
            Guid userId,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize
        )
        {
            var conversations = await _chatService.GetUserConversationsAsync(
                userId,
                pageNumber: pageIndex,
                pageSize: pageSize
            );
            return Ok(conversations);
        }
    }
}
