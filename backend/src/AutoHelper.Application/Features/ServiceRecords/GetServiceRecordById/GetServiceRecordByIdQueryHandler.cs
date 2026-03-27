using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.GetServiceRecordById;

public sealed class GetServiceRecordByIdQueryHandler(
    IServiceRecordRepository serviceRecords) : IRequestHandler<GetServiceRecordByIdQuery, Result<ServiceRecordResponse>>
{
    public async Task<Result<ServiceRecordResponse>> Handle(
        GetServiceRecordByIdQuery request, CancellationToken ct)
    {
        var record = await serviceRecords.GetByIdAsync(request.Id, ct);
        if (record is null)
            return Result<ServiceRecordResponse>.Failure("Service record not found.");

        return Result<ServiceRecordResponse>.Success(new ServiceRecordResponse(
            record.Id,
            record.VehicleId,
            record.Title,
            record.Description,
            record.PerformedAt,
            record.Cost,
            record.ExecutorName,
            record.ExecutorContacts,
            record.Operations,
            record.DocumentUrl));
    }
}
