using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.UpdateServiceRecord;

public sealed class UpdateServiceRecordCommandHandler(
    IServiceRecordRepository serviceRecords,
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateServiceRecordCommand, Result>
{
    public async Task<Result> Handle(UpdateServiceRecordCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var record = await serviceRecords.GetByIdAsync(request.Id, ct);
        if (record is null)
            return AppErrors.ServiceRecord.NotFound;

        var vehicle = await vehicles.GetByIdAsync(record.VehicleId, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return AppErrors.ServiceRecord.AccessDenied;

        record.Update(
            title: request.Title,
            description: request.Description,
            performedAt: request.PerformedAt,
            cost: request.Cost,
            executorName: request.ExecutorName,
            executorContacts: request.ExecutorContacts,
            operations: request.Operations);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
