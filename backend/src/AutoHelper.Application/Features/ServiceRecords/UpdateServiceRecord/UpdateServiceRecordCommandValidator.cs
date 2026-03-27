using FluentValidation;

namespace AutoHelper.Application.Features.ServiceRecords.UpdateServiceRecord;

public sealed class UpdateServiceRecordCommandValidator : AbstractValidator<UpdateServiceRecordCommand>
{
    public UpdateServiceRecordCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.PerformedAt)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("PerformedAt cannot be in the future.");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ExecutorName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.ExecutorContacts)
            .MaximumLength(512)
            .When(x => x.ExecutorContacts is not null);

        RuleFor(x => x.Operations)
            .NotNull()
            .Must(ops => ops.Count > 0)
            .WithMessage("At least one operation must be specified.")
            .ForEach(op => op.NotEmpty().MaximumLength(512));
    }
}
