using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.UnblockCustomer;

public sealed record UnblockCustomerCommand(Guid CustomerId) : IRequest<Result>;
