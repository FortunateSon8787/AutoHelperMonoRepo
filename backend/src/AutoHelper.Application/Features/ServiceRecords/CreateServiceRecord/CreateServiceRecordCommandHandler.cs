using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.ServiceRecords;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.CreateServiceRecord;

public sealed class CreateServiceRecordCommandHandler(
    IServiceRecordRepository serviceRecords,
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateServiceRecordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceRecordCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct);
        if (vehicle is null)
            return AppErrors.Vehicle.NotFound;

        if (vehicle.OwnerId != currentUser.Id.Value)
            return AppErrors.ServiceRecord.AccessDenied;

        var record = ServiceRecord.Create(
            vehicleId: request.VehicleId,
            title: request.Title,
            description: request.Description,
            performedAt: request.PerformedAt,
            cost: request.Cost,
            executorName: request.ExecutorName,
            executorContacts: request.ExecutorContacts,
            operations: request.Operations,
            documentUrl: request.DocumentUrl);

        serviceRecords.Add(record);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(record.Id);
    }
}
