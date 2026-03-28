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
            return Result.Failure("User is not authenticated.");

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);

        if (partner is null)
            return Result.Failure("Partner profile not found.");

        if (!TimeOnly.TryParseExact(request.WorkingOpenFrom, "HH:mm", out var openFrom))
            return Result.Failure("Invalid WorkingOpenFrom format. Expected HH:mm.");

        if (!TimeOnly.TryParseExact(request.WorkingOpenTo, "HH:mm", out var openTo))
            return Result.Failure("Invalid WorkingOpenTo format. Expected HH:mm.");

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
