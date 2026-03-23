using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Auth.Register;

/// <summary>
/// Registers a new customer with email and password.
/// Returns the newly created customer's ID on success.
/// </summary>
public sealed record RegisterCustomerCommand(
    string Name,
    string Email,
    string Password,
    string? Contacts = null) : IRequest<Result<Guid>>;
