using messaging.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace messaging.Presentation.Controllers
{
    [Route("chat")]
    [ApiController]
    public class ChatController(IChatService chatService) : ControllerBase
    {
        private readonly IChatService _chatService = chatService;

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
