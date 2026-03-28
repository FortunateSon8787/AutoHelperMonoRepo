using FluentValidation;

namespace AutoHelper.Application.Features.Chats.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ChatId)
            .NotEmpty().WithMessage("ChatId is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(4000).WithMessage("Message must not exceed 4000 characters.");

        RuleFor(x => x.Locale)
            .NotEmpty().WithMessage("Locale is required.")
            .MaximumLength(10).WithMessage("Locale must not exceed 10 characters.");
    }
}
