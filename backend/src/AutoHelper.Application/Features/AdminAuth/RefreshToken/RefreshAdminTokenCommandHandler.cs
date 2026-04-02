using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Domain.Admins;
using MediatR;

namespace AutoHelper.Application.Features.AdminAuth.RefreshToken;

public sealed class RefreshAdminTokenCommandHandler(
    IAdminRefreshTokenRepository adminRefreshTokens,
    IAdminUserRepository adminUsers,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshAdminTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshAdminTokenCommand request, CancellationToken ct)
    {
        var existing = await adminRefreshTokens.GetByTokenAsync(request.Token, ct);

        if (existing is null || !existing.IsActive)
            return AppErrors.AdminAuth.RefreshTokenInvalid;

        var adminUser = await adminUsers.GetByIdAsync(existing.AdminUserId, ct);
        if (adminUser is null)
            return AppErrors.AdminAuth.RefreshTokenInvalid;

        // Revoke old token
        existing.Revoke();

        // Issue new pair
        var newAccessToken = jwtTokenService.GenerateAdminAccessToken(adminUser);
        var newRawRefreshToken = jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = AdminRefreshToken.Create(
            adminUser.Id,
            newRawRefreshToken,
            jwtTokenService.AdminRefreshTokenExpiryDays);

        adminRefreshTokens.Add(newRefreshToken);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<TokenResponse>.Success(new TokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRawRefreshToken,
            ExpiresAt: newRefreshToken.ExpiresAt));
    }
}
