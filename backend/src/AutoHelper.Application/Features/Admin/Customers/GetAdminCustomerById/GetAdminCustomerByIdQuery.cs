using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.GetAdminCustomerById;

public sealed record GetAdminCustomerByIdQuery(Guid CustomerId) : IRequest<Result<AdminCustomerResponse>>;
