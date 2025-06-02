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

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageToSendDTO messageDto)
        {
            var result = await _chatService.SaveMessageAsync(messageDto);
            return Ok(result);
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
    }
}
