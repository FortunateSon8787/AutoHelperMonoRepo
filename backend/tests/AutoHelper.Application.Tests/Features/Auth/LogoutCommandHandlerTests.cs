using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.Logout;
using AutoHelper.Domain.Customers;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Auth;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _sut = new LogoutCommandHandler(_refreshTokens.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithActiveToken_ShouldRevokeTokenAndSave()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "active-rt", expiryDays: 30);
        _refreshTokens.Setup(r => r.GetByTokenAsync("active-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Handle(new LogoutCommand("active-rt"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        token.IsRevoked.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ShouldSucceedWithoutSaving()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "revoked-rt", expiryDays: 30);
        token.Revoke(); // already revoked

        _refreshTokens.Setup(r => r.GetByTokenAsync("revoked-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Handle(new LogoutCommand("revoked-rt"), CancellationToken.None);

        // Assert — idempotent: success returned, no DB write
        result.IsSuccess.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldReturnFailure()
    {
        // Arrange
        _refreshTokens.Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _sut.Handle(new LogoutCommand("unknown-rt"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
