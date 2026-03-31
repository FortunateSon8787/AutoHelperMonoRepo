using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using MediatR;

namespace AutoHelper.Application.Features.Partners.RegisterPartner;

public sealed class RegisterPartnerCommandHandler(
    IPartnerRepository partners,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterPartnerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterPartnerCommand request, CancellationToken ct)
    {
        if (currentUser.Id is not { } accountUserId)
            return AppErrors.Auth.NotAuthenticated;

        var alreadyExists = await partners.ExistsByAccountUserIdAsync(accountUserId, ct);
        if (alreadyExists)
            return AppErrors.Partner.AlreadyExistsForAccount;

        if (!Enum.TryParse<PartnerType>(request.Type, ignoreCase: true, out var partnerType))
            return AppErrors.Partner.InvalidType;

        var location = GeoPoint.Create(request.LocationLat, request.LocationLng);

        if (!TimeOnly.TryParseExact(request.WorkingOpenFrom, "HH:mm", out var openFrom))
            return AppErrors.Partner.InvalidWorkingOpenFrom;

        if (!TimeOnly.TryParseExact(request.WorkingOpenTo, "HH:mm", out var openTo))
            return AppErrors.Partner.InvalidWorkingOpenTo;

        var workingHours = WorkingSchedule.Create(openFrom, openTo, request.WorkingDays);
        var contacts = PartnerContacts.Create(request.ContactsPhone, request.ContactsWebsite, request.ContactsMessengerLinks);

        var partner = Partner.Create(
            name: request.Name,
            type: partnerType,
            specialization: request.Specialization,
            description: request.Description,
            address: request.Address,
            location: location,
            workingHours: workingHours,
            contacts: contacts,
            accountUserId: accountUserId);

        partners.Add(partner);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(partner.Id);
    }
}
