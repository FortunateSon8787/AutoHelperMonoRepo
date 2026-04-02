using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.AdminAuth.Logout;

public sealed class LogoutAdminCommandHandler(
    IAdminRefreshTokenRepository adminRefreshTokens,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutAdminCommand, Result>
{
    public async Task<Result> Handle(LogoutAdminCommand request, CancellationToken ct)
    {
        var token = await adminRefreshTokens.GetByTokenAsync(request.RefreshToken, ct);

        if (token is null)
            return AppErrors.AdminAuth.RefreshTokenNotFound;

        if (token.IsRevoked)
            return Result.Success(); // already logged out — idempotent

        token.Revoke();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
