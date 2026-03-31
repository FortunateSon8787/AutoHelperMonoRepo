using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.BlockCustomer;

public sealed record BlockCustomerCommand(Guid CustomerId) : IRequest<Result>;
