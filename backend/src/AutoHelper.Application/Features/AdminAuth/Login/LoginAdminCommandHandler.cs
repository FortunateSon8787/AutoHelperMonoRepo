using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Domain.Admins;
using MediatR;

namespace AutoHelper.Application.Features.AdminAuth.Login;

public sealed class LoginAdminCommandHandler(
    IAdminUserRepository adminUsers,
    IAdminRefreshTokenRepository adminRefreshTokens,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginAdminCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginAdminCommand request, CancellationToken ct)
    {
        var adminUser = await adminUsers.GetByEmailAsync(request.Email, ct);

        if (adminUser is null)
            return AppErrors.AdminAuth.InvalidCredentials;

        var passwordValid = passwordHasher.Verify(request.Password, adminUser.PasswordHash);
        if (!passwordValid)
            return AppErrors.AdminAuth.InvalidCredentials;

        var accessToken = jwtTokenService.GenerateAdminAccessToken(adminUser);
        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();

        var refreshToken = AdminRefreshToken.Create(
            adminUser.Id,
            rawRefreshToken,
            jwtTokenService.AdminRefreshTokenExpiryDays);

        adminRefreshTokens.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<TokenResponse>.Success(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            ExpiresAt: refreshToken.ExpiresAt));
    }
}
