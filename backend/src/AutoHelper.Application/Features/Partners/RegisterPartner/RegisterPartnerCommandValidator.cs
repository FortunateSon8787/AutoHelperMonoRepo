using FluentValidation;

namespace AutoHelper.Application.Features.Partners.RegisterPartner;

public sealed class RegisterPartnerCommandValidator : AbstractValidator<RegisterPartnerCommand>
{
    private static readonly string[] ValidPartnerTypes =
        ["AutoService", "CarWash", "Towing", "AutoShop", "Other"];

    public RegisterPartnerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => ValidPartnerTypes.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidPartnerTypes)}.");

        RuleFor(x => x.Specialization)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.LocationLat)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.LocationLng)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.WorkingOpenFrom)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}$")
            .WithMessage("WorkingOpenFrom must be in HH:mm format.");

        RuleFor(x => x.WorkingOpenTo)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}$")
            .WithMessage("WorkingOpenTo must be in HH:mm format.");

        RuleFor(x => x.WorkingDays)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ContactsPhone)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.ContactsWebsite)
            .MaximumLength(2048)
            .When(x => x.ContactsWebsite is not null);

        RuleFor(x => x.ContactsMessengerLinks)
            .MaximumLength(1024)
            .When(x => x.ContactsMessengerLinks is not null);
    }
}
