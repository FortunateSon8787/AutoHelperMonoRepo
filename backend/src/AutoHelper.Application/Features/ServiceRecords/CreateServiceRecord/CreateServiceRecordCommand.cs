using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.CreateServiceRecord;

/// <summary>
/// Creates a service record for a vehicle.
/// The DocumentUrl must point to an already-uploaded PDF (upload is handled at the API layer).
/// </summary>
public sealed record CreateServiceRecordCommand(
    Guid VehicleId,
    string Title,
    string Description,
    DateTime PerformedAt,
    decimal Cost,
    string ExecutorName,
    string? ExecutorContacts,
    List<string> Operations,
    string DocumentUrl) : IRequest<Result<Guid>>;
