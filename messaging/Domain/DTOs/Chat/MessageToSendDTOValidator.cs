using System;
using FluentValidation;

namespace messaging.Domain.DTOs.Chat;

public class MessageToSendDTOValidator : AbstractValidator<MessageToSendDTO>
{
    public MessageToSendDTOValidator()
    {
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(1000);
    }
}
