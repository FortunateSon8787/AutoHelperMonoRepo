using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.DeleteServiceRecord;

public sealed class DeleteServiceRecordCommandHandler(
    IServiceRecordRepository serviceRecords,
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteServiceRecordCommand, Result>
{
    public async Task<Result> Handle(DeleteServiceRecordCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var record = await serviceRecords.GetByIdAsync(request.Id, ct);
        if (record is null)
            return AppErrors.ServiceRecord.NotFound;

        var vehicle = await vehicles.GetByIdAsync(record.VehicleId, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return AppErrors.ServiceRecord.AccessDenied;

        record.Delete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
