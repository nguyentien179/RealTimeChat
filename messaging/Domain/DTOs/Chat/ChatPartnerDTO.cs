using System;

namespace messaging.Domain.DTOs.Chat;

public class ChatPartnerDTO
{
    public Guid PartnerId { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? Timestamp { get; set; }
    public bool HaveUnread { get; set; }
}