namespace AutoHelper.Application.Features.ServiceRecords;

public sealed record ServiceRecordResponse(
    Guid Id,
    Guid VehicleId,
    string Title,
    string Description,
    DateTime PerformedAt,
    decimal Cost,
    string ExecutorName,
    string? ExecutorContacts,
    IReadOnlyList<string> Operations,
    string DocumentUrl);
