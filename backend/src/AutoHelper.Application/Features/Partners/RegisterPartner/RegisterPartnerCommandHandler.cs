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
            return Result<Guid>.Failure("User is not authenticated.");

        var alreadyExists = await partners.ExistsByAccountUserIdAsync(accountUserId, ct);
        if (alreadyExists)
            return Result<Guid>.Failure("A partner profile already exists for this account.");

        if (!Enum.TryParse<PartnerType>(request.Type, ignoreCase: true, out var partnerType))
            return Result<Guid>.Failure($"Invalid partner type: {request.Type}.");

        var location = GeoPoint.Create(request.LocationLat, request.LocationLng);

        if (!TimeOnly.TryParseExact(request.WorkingOpenFrom, "HH:mm", out var openFrom))
            return Result<Guid>.Failure("Invalid WorkingOpenFrom format. Expected HH:mm.");

        if (!TimeOnly.TryParseExact(request.WorkingOpenTo, "HH:mm", out var openTo))
            return Result<Guid>.Failure("Invalid WorkingOpenTo format. Expected HH:mm.");

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
