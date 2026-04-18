using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.RefreshToken;
using AutoHelper.Domain.Customers;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _jwtTokenService.Setup(j => j.RefreshTokenExpiryDays).Returns(30);

        _sut = new RefreshTokenCommandHandler(
            _refreshTokens.Object,
            _customers.Object,
            _jwtTokenService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnNewTokenPairAndRevokeOldToken()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        var oldToken = Domain.Customers.RefreshToken.Create(customer.Id, "old-rt", expiryDays: 30);

        _refreshTokens.Setup(r => r.GetByTokenAsync("old-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldToken);
        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _jwtTokenService.Setup(j => j.GenerateAccessToken(customer)).Returns("new-access-token");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("new-rt");

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("old-rt"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("new-access-token");
        result.Value.RefreshToken.ShouldBe("new-rt");

        // Old token must be revoked (token rotation)
        oldToken.IsRevoked.ShouldBeTrue();

        // New refresh token must be added and changes saved
        _refreshTokens.Verify(
            r => r.Add(It.Is<Domain.Customers.RefreshToken>(rt => rt.Token == "new-rt")),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRevokedToken_ShouldReturnFailure()
    {
        // Arrange
        var token = Domain.Customers.RefreshToken.Create(Guid.NewGuid(), "revoked-rt", expiryDays: 30);
        token.Revoke();

        _refreshTokens.Setup(r => r.GetByTokenAsync("revoked-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("revoked-rt"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange — expiryDays: -1 places ExpiresAt in the past, making IsExpired = true immediately
        var token = Domain.Customers.RefreshToken.Create(Guid.NewGuid(), "expired-rt", expiryDays: -1);

        _refreshTokens.Setup(r => r.GetByTokenAsync("expired-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("expired-rt"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldReturnFailure()
    {
        // Arrange
        _refreshTokens.Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Customers.RefreshToken?)null);

        // Act
        var result = await _sut.Handle(new RefreshTokenCommand("unknown-rt"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }
}
