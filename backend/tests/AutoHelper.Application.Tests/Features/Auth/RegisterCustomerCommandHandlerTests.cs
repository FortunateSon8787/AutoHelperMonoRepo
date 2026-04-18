using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Auth.Register;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Auth;

public class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RegisterCustomerCommandHandler _sut;

    public RegisterCustomerCommandHandlerTests()
    {
        _sut = new RegisterCustomerCommandHandler(
            _customers.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNew_ShouldCreateCustomerAndReturnId()
    {
        // Arrange
        var command = new RegisterCustomerCommand("John Doe", "john@test.com", "Password123!");
        _customers.Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(command.Password))
            .Returns("hashed_password");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNew_ShouldCallRepositoryAddAndSaveChanges()
    {
        // Arrange
        var command = new RegisterCustomerCommand("Jane", "jane@test.com", "Password123!");
        _customers.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — repository must receive the new customer exactly once
        _customers.Verify(
            r => r.Add(It.Is<Domain.Customers.Customer>(c => c.Email == "jane@test.com")),
            Times.Once);

        // Assert — changes must be persisted exactly once
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCustomerCommand("Dup", "dup@test.com", "Password123!");
        _customers.Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();

        // Assert — no customer should be added or saved
        _customers.Verify(r => r.Add(It.IsAny<Domain.Customers.Customer>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHashPasswordBeforeCreatingCustomer()
    {
        // Arrange
        const string plainPassword = "MyP@ssw0rd";
        const string hashedPassword = "hashed_result";

        var command = new RegisterCustomerCommand("Alice", "alice@test.com", plainPassword);
        _customers.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(plainPassword)).Returns(hashedPassword);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert — hash was requested with the original plain-text password
        _passwordHasher.Verify(h => h.Hash(plainPassword), Times.Once);

        // Assert — the customer stored in the repo has the hashed password, not plain text
        _customers.Verify(
            r => r.Add(It.Is<Domain.Customers.Customer>(c => c.PasswordHash == hashedPassword)),
            Times.Once);
    }
}
