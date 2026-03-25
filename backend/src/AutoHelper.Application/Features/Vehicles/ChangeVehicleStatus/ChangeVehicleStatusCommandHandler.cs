using AutoHelper.Application.Common.Interfaces;
using MediatR;
using AppResult = AutoHelper.Application.Common.Result;

namespace AutoHelper.Application.Features.Vehicles.ChangeVehicleStatus;

public sealed class ChangeVehicleStatusCommandHandler(
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeVehicleStatusCommand, AppResult>
{
    public async Task<AppResult> Handle(ChangeVehicleStatusCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppResult.Failure("User is not authenticated.");

        var vehicle = await vehicles.GetByIdAsync(request.Id, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return AppResult.Failure("Vehicle not found.");

        var domainResult = vehicle.ChangeStatus(request.Status, request.PartnerName, request.DocumentUrl);
        if (domainResult.IsFailure)
            return AppResult.Failure(domainResult.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        return AppResult.Success();
    }
}
