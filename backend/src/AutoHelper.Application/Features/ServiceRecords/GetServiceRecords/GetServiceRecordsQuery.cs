using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.GetServiceRecords;

/// <summary>Returns all non-deleted service records for a vehicle.</summary>
public sealed record GetServiceRecordsQuery(Guid VehicleId) : IRequest<Result<IReadOnlyList<ServiceRecordResponse>>>;
