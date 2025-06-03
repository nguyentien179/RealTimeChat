using messaging.Application.Interfaces;
using messaging.Application.Mappers;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.Entity;
using messaging.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace messaging.Presentation.Controllers
{
    [Route("chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("private")]
        public async Task<IActionResult> GetPrivateMessages(
            [FromQuery] Guid user1,
            [FromQuery] Guid user2
        )
        {
            var messages = await _chatService.GetPrivateMessagesAsync(user1, user2);
            return Ok(messages);
        }

        [HttpGet("group")]
        public async Task<IActionResult> GetGroupMessages([FromQuery] string chatRoom)
        {
            var messages = await _chatService.GetGroupMessagesAsync(chatRoom);
            return Ok(messages);
        }

        [HttpGet("partners/{userId:guid}")]
        public async Task<IActionResult> GetChatPartners(Guid userId)
        {
            var partners = await _chatService.GetChatPartnersAsync(userId);
            return Ok(partners);
        }
        [HttpGet("groups/{userId:guid}")]
        public async Task<IActionResult> GetUserGroups(Guid userId)
        {
            var groups = await _chatService.GetUserChatRoomsAsync(userId);
            return Ok(groups);
        }
    }
}
