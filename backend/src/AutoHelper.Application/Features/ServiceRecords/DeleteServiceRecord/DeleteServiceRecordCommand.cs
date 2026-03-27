using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.ServiceRecords.DeleteServiceRecord;

public sealed record DeleteServiceRecordCommand(Guid Id) : IRequest<Result>;
