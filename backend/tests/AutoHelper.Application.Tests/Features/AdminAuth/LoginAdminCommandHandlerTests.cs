using AutoFixture;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.AdminAuth.Login;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Domain.Admins;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.AdminAuth;

public class LoginAdminCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAdminUserRepository> _adminUsers = new();
    private readonly Mock<IAdminRefreshTokenRepository> _adminRefreshTokens = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LoginAdminCommandHandler _sut;

    public LoginAdminCommandHandlerTests()
    {
        _jwtTokenService.Setup(j => j.AdminRefreshTokenExpiryDays).Returns(7);

        _sut = new LoginAdminCommandHandler(
            _adminUsers.Object,
            _adminRefreshTokens.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var adminUser = AdminUser.Create("admin@test.com", "hashed", AdminRole.Admin);
        _adminUsers.Setup(r => r.GetByEmailAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _passwordHasher.Setup(h => h.Verify("Password1!", "hashed")).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateAdminAccessToken(adminUser)).Returns("admin.access.token");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("admin-refresh-token");

        var command = new LoginAdminCommand("admin@test.com", "Password1!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("admin.access.token");
        result.Value.RefreshToken.ShouldBe("admin-refresh-token");
        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldPersistRefreshTokenAndSave()
    {
        // Arrange
        var adminUser = AdminUser.Create("superadmin@test.com", "hashed", AdminRole.SuperAdmin);
        _adminUsers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateAdminAccessToken(It.IsAny<AdminUser>())).Returns("at");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("rt");

        // Act
        await _sut.Handle(new LoginAdminCommand("superadmin@test.com", "pass"), CancellationToken.None);

        // Assert — refresh token must be persisted with correct admin user ID
        _adminRefreshTokens.Verify(
            r => r.Add(It.Is<AdminRefreshToken>(rt =>
                rt.Token == "rt" &&
                rt.AdminUserId == adminUser.Id &&
                !rt.IsRevoked)),
            Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdminNotFound_ShouldReturnFailure()
    {
        // Arrange
        _adminUsers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminUser?)null);

        var command = new LoginAdminCommand("ghost@test.com", "Password1!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _adminRefreshTokens.Verify(r => r.Add(It.IsAny<AdminRefreshToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldReturnFailure()
    {
        // Arrange
        var adminUser = AdminUser.Create("admin@test.com", "hashed", AdminRole.Admin);
        _adminUsers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await _sut.Handle(new LoginAdminCommand("admin@test.com", "WrongPass"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _adminRefreshTokens.Verify(r => r.Add(It.IsAny<AdminRefreshToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotLeakWhetherEmailOrPasswordIsWrong()
    {
        // Both "not found" and "wrong password" should return the same error code
        // to prevent user enumeration attacks

        var adminUser = AdminUser.Create("admin@test.com", "hashed", AdminRole.Admin);
        _adminUsers.Setup(r => r.GetByEmailAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        _adminUsers.Setup(r => r.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminUser?)null);

        var wrongPassResult = await _sut.Handle(
            new LoginAdminCommand("admin@test.com", "WrongPass"), CancellationToken.None);
        var notFoundResult = await _sut.Handle(
            new LoginAdminCommand("nonexistent@test.com", "anypass"), CancellationToken.None);

        // Both must return the same error code
        wrongPassResult.Error!.Code.ShouldBe(notFoundResult.Error!.Code);
    }
}
