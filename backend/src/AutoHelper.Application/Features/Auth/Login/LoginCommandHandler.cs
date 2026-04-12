using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    ICustomerRepository customers,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var customer = await customers.GetByEmailAsync(request.Email, ct);

        if (customer is null || customer.PasswordHash is null)
            return AppErrors.Auth.InvalidCredentials;

        var passwordValid = passwordHasher.Verify(request.Password, customer.PasswordHash);
        if (!passwordValid)
            return AppErrors.Auth.InvalidCredentials;

        // Revoke all previously active tokens for this customer to prevent unlimited token accumulation.
        var existingTokens = await refreshTokens.GetByCustomerIdAsync(customer.Id, ct);
        foreach (var token in existingTokens)
        {
            if (token.IsActive)
                token.Revoke();
        }

        var accessToken = jwtTokenService.GenerateAccessToken(customer);
        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();

        var refreshToken = global::AutoHelper.Domain.Customers.RefreshToken.Create(
            customerId: customer.Id,
            token: rawRefreshToken,
            expiryDays: jwtTokenService.RefreshTokenExpiryDays);

        refreshTokens.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<TokenResponse>.Success(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            ExpiresAt: refreshToken.ExpiresAt));
    }
}
