using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Partners;
using AutoHelper.Application.Features.Partners.RegisterPartner;
using AutoHelper.Domain.Partners;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Partners;

public class RegisterPartnerCommandHandlerTests
{
    private readonly Mock<IPartnerRepository> _partners = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RegisterPartnerCommandHandler _sut;

    public RegisterPartnerCommandHandlerTests()
    {
        _sut = new RegisterPartnerCommandHandler(
            _partners.Object,
            _currentUser.Object,
            _unitOfWork.Object);

        // Default: authenticated user
        _currentUser.Setup(u => u.Id).Returns(Guid.NewGuid());
    }

    private RegisterPartnerCommand ValidCommand() => new(
        Name: "Auto Fix",
        Type: "AutoService",
        Specialization: "Diagnostics",
        Description: "Full service auto repair shop",
        Address: "St. Petersburg, Nevsky 10",
        LocationLat: 59.93,
        LocationLng: 30.31,
        WorkingOpenFrom: "09:00",
        WorkingOpenTo: "18:00",
        WorkingDays: "Mon-Fri",
        ContactsPhone: "+7-999-100-00-00");

    [Fact]
    public async Task Handle_WhenNoExistingProfile_ShouldCreatePartnerAndReturnId()
    {
        _partners.Setup(r => r.ExistsByAccountUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenNoExistingProfile_ShouldCallRepositoryAddAndSaveChanges()
    {
        _partners.Setup(r => r.ExistsByAccountUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        _partners.Verify(r => r.Add(It.IsAny<Partner>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPartnerAlreadyExists_ShouldReturnFailure()
    {
        _partners.Setup(r => r.ExistsByAccountUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();

        _partners.Verify(r => r.Add(It.IsAny<Partner>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _partners.Verify(r => r.Add(It.IsAny<Partner>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvalidPartnerType_ShouldReturnFailure()
    {
        _partners.Setup(r => r.ExistsByAccountUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = ValidCommand() with { Type = "InvalidType" };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _partners.Verify(r => r.Add(It.IsAny<Partner>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreatePartnerWithIsVerifiedFalseAndIsActiveFalse()
    {
        _partners.Setup(r => r.ExistsByAccountUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Partner? capturedPartner = null;
        _partners.Setup(r => r.Add(It.IsAny<Partner>()))
            .Callback<Partner>(p => capturedPartner = p);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        capturedPartner.ShouldNotBeNull();
        capturedPartner!.IsVerified.ShouldBeFalse();
        capturedPartner.IsActive.ShouldBeFalse();
    }
}
