using AutoFixture;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Domain.Customers;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _jwtTokenService.Setup(j => j.RefreshTokenExpiryDays).Returns(30);

        _sut = new LoginCommandHandler(
            _customers.Object,
            _refreshTokens.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hashed");
        _customers.Setup(r => r.GetByEmailAsync("alice@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _passwordHasher.Setup(h => h.Verify("Password1!", "hashed")).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateAccessToken(customer)).Returns("access.token");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token-value");

        var command = new LoginCommand("alice@test.com", "Password1!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("access.token");
        result.Value.RefreshToken.ShouldBe("refresh-token-value");
        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldStoreRefreshTokenAndSave()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Bob", "bob@test.com", "hashed");
        _customers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateAccessToken(It.IsAny<Customer>())).Returns("at");
        _jwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns("rt");

        // Act
        await _sut.Handle(new LoginCommand("bob@test.com", "pass"), CancellationToken.None);

        // Assert — refresh token must be persisted
        _refreshTokens.Verify(
            r => r.Add(It.Is<RefreshToken>(rt => rt.Token == "rt" && rt.CustomerId == customer.Id)),
            Times.Once);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        _customers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new LoginCommand("ghost@test.com", "Password1!");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldReturnFailure()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Carol", "carol@test.com", "hashed");
        _customers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await _sut.Handle(new LoginCommand("carol@test.com", "WrongPassword"), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
