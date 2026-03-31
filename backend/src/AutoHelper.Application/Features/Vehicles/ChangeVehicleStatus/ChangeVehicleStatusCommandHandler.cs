using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Vehicles;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.ChangeVehicleStatus;

public sealed class ChangeVehicleStatusCommandHandler(
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeVehicleStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeVehicleStatusCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var vehicle = await vehicles.GetByIdAsync(request.Id, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return AppErrors.Vehicle.NotFound;

        var domainResult = vehicle.ChangeStatus(request.Status, request.PartnerName, request.DocumentUrl);
        if (domainResult.IsFailure)
            return MapDomainVehicleError(domainResult.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static AppError MapDomainVehicleError(string domainError) => domainError switch
    {
        _ when domainError.Contains("Partner name") => AppErrors.Vehicle.PartnerNameRequiredForInRepair,
        _ when domainError.Contains("Document") => AppErrors.Vehicle.DocumentRequiredForRecycledOrDismantled,
        _ => new AppError("VEHICLE_DOMAIN", domainError)
    };
}
