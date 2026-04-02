using AutoFixture;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.AdminAuth.RefreshToken;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Domain.Admins;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.AdminAuth;

public class RefreshAdminTokenCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAdminRefreshTokenRepository> _adminRefreshTokens = new();
    private readonly Mock<IAdminUserRepository> _adminUsers = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshAdminTokenCommandHandler _sut;

    public RefreshAdminTokenCommandHandlerTests()
    {
        _jwtTokenService.Setup(j => j.AdminRefreshTokenExpiryDays).Returns(7);

        _sut = new RefreshAdminTokenCommandHandler(
            _adminRefreshTokens.Object,
            _adminUsers.Object,
            _jwtTokenService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnNewTokenPair()
    {
        // Arrange
        var adminUser = AdminUser.Create("admin@test.com", "hash", AdminRole.Admin);
        var existingToken = AdminRefreshToken.Create(adminUser.Id, "old-token", 7);

        _adminRefreshTokens.Setup(r => r.GetByTokenAsync("old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);
        _adminUsers.Setup(r => r.GetByIdAsync(adminUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _jwtTokenService.Setup(j => j.GenerateAdminAccessToken(adminUser)).Returns("new.access.token");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");

        // Act
        var result = await _sut.Handle(new RefreshAdminTokenCommand("old-token"), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("new.access.token");
        result.Value.RefreshToken.ShouldBe("new-refresh-token");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldRevokeOldAndPersistNew()
    {
        // Arrange
        var adminUser = AdminUser.Create("admin@test.com", "hash", AdminRole.Admin);
        var existingToken = AdminRefreshToken.Create(adminUser.Id, "old-token", 7);

        _adminRefreshTokens.Setup(r => r.GetByTokenAsync("old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);
        _adminUsers.Setup(r => r.GetByIdAsync(adminUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _jwtTokenService.Setup(j => j.GenerateAdminAccessToken(It.IsAny<AdminUser>())).Returns("at");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("new-rt");

        // Act
        await _sut.Handle(new RefreshAdminTokenCommand("old-token"), CancellationToken.None);

        // Assert — old token is revoked
        existingToken.IsRevoked.ShouldBeTrue();

        // Assert — new token is persisted
        _adminRefreshTokens.Verify(
            r => r.Add(It.Is<AdminRefreshToken>(rt => rt.Token == "new-rt" && rt.AdminUserId == adminUser.Id)),
            Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldReturnFailure()
    {
        // Arrange
        _adminRefreshTokens.Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminRefreshToken?)null);

        // Act
        var result = await _sut.Handle(new RefreshAdminTokenCommand("nonexistent"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenIsRevoked_ShouldReturnFailure()
    {
        // Arrange
        var adminUser = AdminUser.Create("admin@test.com", "hash", AdminRole.Admin);
        var revokedToken = AdminRefreshToken.Create(adminUser.Id, "revoked-token", 7);
        revokedToken.Revoke();

        _adminRefreshTokens.Setup(r => r.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        // Act
        var result = await _sut.Handle(new RefreshAdminTokenCommand("revoked-token"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
