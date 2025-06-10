using System.Reflection;
using System.Security.Claims;
using messaging.Application.Common;
using messaging.Application.Interfaces;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace messaging.Presentation.Controllers
{
    [Route("chat")]
    [ApiController]
    public class ChatController(IChatService chatService, IHttpContextAccessor httpContextAccessor)
        : ControllerBase
    {
        private readonly IChatService _chatService = chatService;
        private readonly IHttpContextAccessor _httpContextaccessor = httpContextAccessor;

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
                pageIndex: pageIndex,
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

        [Authorize]
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadMessagesCount()
        {
            var user = _httpContextaccessor.HttpContext?.User;

            var userIdClaim = user?.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            Guid userId = Guid.Parse(userIdClaim.Value);
            int count = await _chatService.CountUnreadMessagesAsync(userId);

            return Ok(new { count });
        }
    }
}
