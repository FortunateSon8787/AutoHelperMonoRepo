using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Partners.GetPartnerById;
using AutoHelper.Domain.Partners;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Partners;

public class GetPartnerByIdQueryHandlerTests
{
    private readonly Mock<IPartnerRepository> _partners = new();
    private readonly GetPartnerByIdQueryHandler _sut;

    public GetPartnerByIdQueryHandlerTests()
    {
        _sut = new GetPartnerByIdQueryHandler(_partners.Object);
    }

    private static Partner CreateVerifiedPartner()
    {
        var partner = Partner.Create(
            name: "Verified Auto",
            type: PartnerType.AutoService,
            specialization: "Diagnostics",
            description: "Full service center",
            address: "Moscow, Lenina 1",
            location: GeoPoint.Create(55.75, 37.61),
            workingHours: WorkingSchedule.Create(new TimeOnly(9, 0), new TimeOnly(18, 0), "Mon-Fri"),
            contacts: PartnerContacts.Create("+7-999-000-0000"),
            accountUserId: Guid.NewGuid());

        partner.Verify();
        return partner;
    }

    private static Partner CreateUnverifiedPartner()
    {
        return Partner.Create(
            name: "Pending Auto",
            type: PartnerType.AutoService,
            specialization: "Diagnostics",
            description: "Awaiting verification",
            address: "Moscow, Lenina 2",
            location: GeoPoint.Create(55.75, 37.61),
            workingHours: WorkingSchedule.Create(new TimeOnly(9, 0), new TimeOnly(18, 0), "Mon-Fri"),
            contacts: PartnerContacts.Create("+7-999-111-1111"),
            accountUserId: Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_WhenPartnerExistsAndIsVerifiedAndActive_ShouldReturnPartnerResponse()
    {
        var partner = CreateVerifiedPartner();
        _partners.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var result = await _sut.Handle(new GetPartnerByIdQuery(partner.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(partner.Id);
        result.Value.Name.ShouldBe(partner.Name);
        result.Value.IsVerified.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenPartnerNotFound_ShouldReturnFailure()
    {
        _partners.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var result = await _sut.Handle(new GetPartnerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenPartnerIsNotVerified_ShouldReturnFailure()
    {
        var unverified = CreateUnverifiedPartner();
        _partners.Setup(r => r.GetByIdAsync(unverified.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unverified);

        var result = await _sut.Handle(new GetPartnerByIdQuery(unverified.Id), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenPartnerIsVerifiedButDeactivated_ShouldReturnFailure()
    {
        var partner = CreateVerifiedPartner();
        partner.Deactivate();

        _partners.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var result = await _sut.Handle(new GetPartnerByIdQuery(partner.Id), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenPartnerFound_ShouldMapAllFieldsCorrectly()
    {
        var partner = CreateVerifiedPartner();
        _partners.Setup(r => r.GetByIdAsync(partner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var result = await _sut.Handle(new GetPartnerByIdQuery(partner.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        response.Id.ShouldBe(partner.Id);
        response.Name.ShouldBe(partner.Name);
        response.Type.ShouldBe(partner.Type.ToString());
        response.Address.ShouldBe(partner.Address);
        response.LocationLat.ShouldBe(partner.Location.Lat);
        response.LocationLng.ShouldBe(partner.Location.Lng);
        response.ContactsPhone.ShouldBe(partner.Contacts.Phone);
    }
}
