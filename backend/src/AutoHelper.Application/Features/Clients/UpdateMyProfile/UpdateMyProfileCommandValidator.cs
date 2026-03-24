using FluentValidation;

namespace AutoHelper.Application.Features.Clients.UpdateMyProfile;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Contacts)
            .MaximumLength(512).WithMessage("Contacts must not exceed 512 characters.")
            .When(x => x.Contacts is not null);
    }
}
