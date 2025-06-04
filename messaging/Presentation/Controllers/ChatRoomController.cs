using messaging.Application.Common;
using messaging.Application.Interfaces;
using messaging.Domain.DTOs.Chat;
using messaging.Domain.DTOs.ChatRoom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace messaging.Presentation.Controllers
{
    [Route("chatrooms")]
    [ApiController]
    public class ChatRoomsController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;

        public ChatRoomsController(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }

        [HttpPost]
        public async Task<ActionResult<ChatRoomToReturnDTO>> CreateChatRoom(ChatRoomToAddDTO dto)
        {
            try
            {
                var result = await _chatRoomService.CreateChatRoomAsync(dto);
                return CreatedAtAction(nameof(CreateChatRoom), result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("add-users")]
        public async Task<IActionResult> AddUsersToChatRoom(AddUserToRoomDTO dto)
        {
            try
            {
                await _chatRoomService.AddUsersToChatRoomAsync(dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("kick-user")]
        public async Task<IActionResult> KickUserFromRoom(
            [FromQuery] Guid chatRoomId,
            [FromQuery] Guid userIdToKick
        )
        {
            try
            {
                await _chatRoomService.KickUserFromRoomAsync(chatRoomId, userIdToKick);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leave")]
        public async Task<IActionResult> LeaveChatRoom(
            [FromQuery] Guid chatRoomId,
            [FromQuery] Guid userId
        )
        {
            try
            {
                await _chatRoomService.LeaveChatRoomAsync(chatRoomId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ChatRoomToReturnDTO>> UpdateChatRoom(ChatRoomToUpdateDTO dto)
        {
            try
            {
                var result = await _chatRoomService.UpdateChatRoomAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<MessageToReturnDTO>>> GetGroupInfo(
            [FromQuery] Guid roomId,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize
        )
        {
            try
            {
                var result = await _chatRoomService.GetChatRoomByIdAsync(
                    roomId,
                    pageIndex,
                    pageIndex
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
