using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var token = await refreshTokens.GetByTokenAsync(request.RefreshToken, ct);

        if (token is null)
            return Result.Failure("Refresh token not found.");

        if (token.IsRevoked)
            return Result.Success(); // already logged out — idempotent

        token.Revoke();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
