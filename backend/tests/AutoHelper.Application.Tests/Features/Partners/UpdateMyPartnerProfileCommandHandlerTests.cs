using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Partners.UpdateMyPartnerProfile;
using AutoHelper.Domain.Partners;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Partners;

public class UpdateMyPartnerProfileCommandHandlerTests
{
    private readonly Mock<IPartnerRepository> _partners = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateMyPartnerProfileCommandHandler _sut;

    private readonly Guid _userId = Guid.NewGuid();

    public UpdateMyPartnerProfileCommandHandlerTests()
    {
        _sut = new UpdateMyPartnerProfileCommandHandler(
            _partners.Object, _currentUser.Object, _unitOfWork.Object);

        _currentUser.Setup(u => u.Id).Returns(_userId);
    }

    private Partner CreatePartner() => Partner.Create(
        "Old Name", PartnerType.AutoService, "Old Spec", "Old Desc", "Old Addr",
        GeoPoint.Create(55.0, 37.0),
        WorkingSchedule.Create(new TimeOnly(9, 0), new TimeOnly(18, 0), "Mon-Fri"),
        PartnerContacts.Create("+7-999-000-00-00"),
        _userId);

    private UpdateMyPartnerProfileCommand ValidCommand() => new(
        Name: "New Name",
        Specialization: "New Spec",
        Description: "New Description",
        Address: "New Address",
        LocationLat: 59.93,
        LocationLng: 30.31,
        WorkingOpenFrom: "08:00",
        WorkingOpenTo: "20:00",
        WorkingDays: "Mon-Sun",
        ContactsPhone: "+7-800-100-00-00");

    [Fact]
    public async Task Handle_WhenPartnerExists_ShouldUpdateProfileAndSave()
    {
        var partner = CreatePartner();
        _partners.Setup(r => r.GetByAccountUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        partner.Name.ShouldBe("New Name");
        partner.Specialization.ShouldBe("New Spec");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPartnerNotFound_ShouldReturnFailure()
    {
        _partners.Setup(r => r.GetByAccountUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
    }
}
