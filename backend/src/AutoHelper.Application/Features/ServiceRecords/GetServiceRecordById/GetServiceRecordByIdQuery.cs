using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.GetServiceRecordById;

public sealed record GetServiceRecordByIdQuery(Guid Id) : IRequest<Result<ServiceRecordResponse>>;
