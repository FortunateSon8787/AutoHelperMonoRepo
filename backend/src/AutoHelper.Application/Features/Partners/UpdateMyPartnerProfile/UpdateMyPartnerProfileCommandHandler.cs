using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using MediatR;

namespace AutoHelper.Application.Features.Partners.UpdateMyPartnerProfile;

public sealed class UpdateMyPartnerProfileCommandHandler(
    IPartnerRepository partners,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMyPartnerProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateMyPartnerProfileCommand request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return AppErrors.Auth.NotAuthenticated;

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);

        if (partner is null)
            return AppErrors.Partner.ProfileNotFound;

        if (!TimeOnly.TryParseExact(request.WorkingOpenFrom, "HH:mm", out var openFrom))
            return AppErrors.Partner.InvalidWorkingOpenFrom;

        if (!TimeOnly.TryParseExact(request.WorkingOpenTo, "HH:mm", out var openTo))
            return AppErrors.Partner.InvalidWorkingOpenTo;

        var location = GeoPoint.Create(request.LocationLat, request.LocationLng);
        var workingHours = WorkingSchedule.Create(openFrom, openTo, request.WorkingDays);
        var contacts = PartnerContacts.Create(request.ContactsPhone, request.ContactsWebsite, request.ContactsMessengerLinks);

        partner.UpdateProfile(
            name: request.Name,
            specialization: request.Specialization,
            description: request.Description,
            address: request.Address,
            location: location,
            workingHours: workingHours,
            contacts: contacts);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
