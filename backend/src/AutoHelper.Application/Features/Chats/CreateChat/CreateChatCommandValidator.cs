using FluentValidation;

namespace AutoHelper.Application.Features.Chats.CreateChat;

public sealed class CreateChatCommandValidator : AbstractValidator<CreateChatCommand>
{
    public CreateChatCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Chat title is required.")
            .MaximumLength(200).WithMessage("Chat title must not exceed 200 characters.");

        RuleFor(x => x.Mode)
            .IsInEnum().WithMessage("Invalid chat mode.");
    }
}
