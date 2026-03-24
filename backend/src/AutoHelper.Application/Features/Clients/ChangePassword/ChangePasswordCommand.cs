using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.ChangePassword;

/// <summary>
/// Changes the password of the currently authenticated local-auth customer.
/// </summary>
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Result>;
