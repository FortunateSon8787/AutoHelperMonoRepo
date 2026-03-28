using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Partners.VerifyPartner;
using AutoHelper.Domain.Partners;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Partners;

public class VerifyPartnerCommandHandlerTests
{
    private readonly Mock<IPartnerRepository> _partners = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly VerifyPartnerCommandHandler _sut;

    public VerifyPartnerCommandHandlerTests()
    {
        _sut = new VerifyPartnerCommandHandler(_partners.Object, _unitOfWork.Object);
    }

    private static Partner CreateUnverifiedPartner()
    {
        return Partner.Create(
            "Shop", PartnerType.CarWash, "spec", "desc", "addr",
            GeoPoint.Create(55.0, 37.0),
            WorkingSchedule.Create(new TimeOnly(9, 0), new TimeOnly(18, 0), "Mon-Fri"),
            PartnerContacts.Create("+7-999-000-00-00"),
            Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_WhenPartnerExists_ShouldVerifyAndSave()
    {
        var partner = CreateUnverifiedPartner();
        _partners.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var result = await _sut.Handle(new VerifyPartnerCommand(partner.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        partner.IsVerified.ShouldBeTrue();
        partner.IsActive.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPartnerNotFound_ShouldReturnFailure()
    {
        _partners.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var result = await _sut.Handle(new VerifyPartnerCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
