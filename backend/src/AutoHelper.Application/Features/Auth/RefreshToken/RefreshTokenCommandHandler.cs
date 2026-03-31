using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ICustomerRepository customers,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existingToken = await refreshTokens.GetByTokenAsync(request.Token, ct);

        if (existingToken is null || !existingToken.IsActive)
            return AppErrors.Auth.RefreshTokenInvalid;

        var customer = await customers.GetByIdAsync(existingToken.CustomerId, ct);
        if (customer is null)
            return AppErrors.Customer.NotFound;

        // Token rotation: revoke the old token and issue a new pair
        existingToken.Revoke();

        var newRawRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newRefreshToken = Domain.Customers.RefreshToken.Create(
            customerId: customer.Id,
            token: newRawRefreshToken,
            expiryDays: jwtTokenService.RefreshTokenExpiryDays);

        refreshTokens.Add(newRefreshToken);
        await unitOfWork.SaveChangesAsync(ct);

        var newAccessToken = jwtTokenService.GenerateAccessToken(customer);

        return Result<TokenResponse>.Success(new TokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRawRefreshToken,
            ExpiresAt: newRefreshToken.ExpiresAt));
    }
}
