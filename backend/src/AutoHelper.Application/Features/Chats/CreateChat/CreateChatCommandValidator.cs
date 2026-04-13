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

        When(x => x.PartnerAdviceInput is not null, () =>
        {
            RuleFor(x => x.PartnerAdviceInput!.Request)
                .NotEmpty().WithMessage("Service request description is required.")
                .MaximumLength(500).WithMessage("Service request must not exceed 500 characters.");

            RuleFor(x => x.PartnerAdviceInput!.Urgency)
                .MaximumLength(100).WithMessage("Urgency must not exceed 100 characters.")
                .When(x => x.PartnerAdviceInput!.Urgency is not null);
        });
    }
}
