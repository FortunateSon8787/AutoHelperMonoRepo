using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.UpdateServiceRecord;

public sealed record UpdateServiceRecordCommand(
    Guid Id,
    string Title,
    string Description,
    DateTime PerformedAt,
    decimal Cost,
    string ExecutorName,
    string? ExecutorContacts,
    List<string> Operations) : IRequest<Result>;
