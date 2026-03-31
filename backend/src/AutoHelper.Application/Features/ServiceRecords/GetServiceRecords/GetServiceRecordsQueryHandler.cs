using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.GetServiceRecords;

public sealed class GetServiceRecordsQueryHandler(
    IServiceRecordRepository serviceRecords,
    IVehicleRepository vehicles) : IRequestHandler<GetServiceRecordsQuery, Result<IReadOnlyList<ServiceRecordResponse>>>
{
    public async Task<Result<IReadOnlyList<ServiceRecordResponse>>> Handle(
        GetServiceRecordsQuery request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.VehicleId, ct);
        if (vehicle is null)
            return AppErrors.Vehicle.NotFound;

        var records = await serviceRecords.GetByVehicleIdAsync(request.VehicleId, ct);

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
