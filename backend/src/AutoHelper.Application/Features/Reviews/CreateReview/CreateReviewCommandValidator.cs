using FluentValidation;

namespace AutoHelper.Application.Features.Reviews.CreateReview;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Basis)
            .NotEmpty();

        RuleFor(x => x.InteractionReferenceId)
            .NotEmpty();
    }
}
