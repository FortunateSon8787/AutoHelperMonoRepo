using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.GetPublicServiceRecords;

public sealed class GetPublicServiceRecordsQueryHandler(
    IServiceRecordRepository serviceRecords,
    IVehicleRepository vehicles) : IRequestHandler<GetPublicServiceRecordsQuery, Result<IReadOnlyList<ServiceRecordResponse>>>
{
    public async Task<Result<IReadOnlyList<ServiceRecordResponse>>> Handle(
        GetPublicServiceRecordsQuery request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByVinAsync(request.Vin, ct);
        if (vehicle is null)
            return AppErrors.Vehicle.NotFound;

        var records = await serviceRecords.GetByVehicleIdAsync(vehicle.Id, ct);

        var response = records
            .Select(r => new ServiceRecordResponse(
                r.Id,
                r.VehicleId,
                r.Title,
                r.Description,
                r.PerformedAt,
                r.Cost,
                r.ExecutorName,
                r.ExecutorContacts,
                r.Operations,
                r.DocumentUrl))
            .ToList();

        return Result<IReadOnlyList<ServiceRecordResponse>>.Success(response);
    }
}
