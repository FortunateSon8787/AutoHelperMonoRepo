using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.GetPublicServiceRecords;

/// <summary>
/// Returns the public service history for a vehicle identified by VIN.
/// Accessible without authentication — history is public per domain rules.
/// </summary>
public sealed record GetPublicServiceRecordsQuery(string Vin)
    : IRequest<Result<IReadOnlyList<ServiceRecordResponse>>>;
