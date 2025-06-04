using System;
using System.Text.Json.Serialization;

namespace messaging.Domain.DTOs.Chat;

public class MessageToSendDTO
{
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public string Content { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
