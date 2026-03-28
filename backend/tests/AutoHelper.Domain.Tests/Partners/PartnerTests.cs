using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.Partners;
using AutoHelper.Domain.Partners.Events;
using Shouldly;

namespace AutoHelper.Domain.Tests.Partners;

public class PartnerTests : TestBase
{
    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static GeoPoint ValidLocation() => GeoPoint.Create(55.75, 37.61);

    private static WorkingSchedule ValidSchedule() =>
        WorkingSchedule.Create(new TimeOnly(9, 0), new TimeOnly(18, 0), "Mon-Fri");

    private static PartnerContacts ValidContacts() =>
        PartnerContacts.Create("+7-999-000-00-00", "https://example.com");

    private static Partner CreateValidPartner(Guid? accountUserId = null) =>
        Partner.Create(
            name: "FixIT AutoService",
            type: PartnerType.AutoService,
            specialization: "Engine repair and diagnostics",
            description: "Professional engine repair shop with 10 years of experience.",
            address: "Moscow, Lenina St. 1",
            location: ValidLocation(),
            workingHours: ValidSchedule(),
            contacts: ValidContacts(),
            accountUserId: accountUserId ?? Guid.NewGuid());

    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldCreatePartner()
    {
        var accountUserId = Guid.NewGuid();

        var partner = CreateValidPartner(accountUserId);

        partner.Id.ShouldNotBe(Guid.Empty);
        partner.Name.ShouldBe("FixIT AutoService");
        partner.Type.ShouldBe(PartnerType.AutoService);
        partner.Specialization.ShouldBe("Engine repair and diagnostics");
        partner.Address.ShouldBe("Moscow, Lenina St. 1");
        partner.AccountUserId.ShouldBe(accountUserId);
        partner.IsVerified.ShouldBeFalse();
        partner.IsActive.ShouldBeFalse();
        partner.IsPotentiallyUnfit.ShouldBeFalse();
        partner.ShowBannersToAnonymous.ShouldBeFalse();
        partner.IsDeleted.ShouldBeFalse();
        partner.LogoUrl.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldRaisePartnerRegisteredEvent()
    {
        var accountUserId = Guid.NewGuid();

        var partner = CreateValidPartner(accountUserId);

        var evt = partner.DomainEvents.OfType<PartnerRegisteredEvent>().SingleOrDefault();
        evt.ShouldNotBeNull();
        evt.PartnerId.ShouldBe(partner.Id);
        evt.AccountUserId.ShouldBe(accountUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyName_ShouldThrowDomainException(string name)
    {
        Should.Throw<DomainException>(() =>
            Partner.Create(name, PartnerType.AutoService, "spec", "desc", "addr",
                ValidLocation(), ValidSchedule(), ValidContacts(), Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            Partner.Create("Name", PartnerType.AutoService, "spec", "desc", "addr",
                ValidLocation(), ValidSchedule(), ValidContacts(), Guid.Empty));
    }

    // ─── Verify ───────────────────────────────────────────────────────────────

    [Fact]
    public void Verify_ShouldSetIsVerifiedAndIsActiveToTrue()
    {
        var partner = CreateValidPartner();
        partner.IsVerified.ShouldBeFalse();
        partner.IsActive.ShouldBeFalse();

        partner.Verify();

        partner.IsVerified.ShouldBeTrue();
        partner.IsActive.ShouldBeTrue();
    }

    // ─── Deactivate ───────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_AfterVerify_ShouldSetIsActiveToFalse()
    {
        var partner = CreateValidPartner();
        partner.Verify();
        partner.IsActive.ShouldBeTrue();

        partner.Deactivate();

        partner.IsActive.ShouldBeFalse();
        partner.IsVerified.ShouldBeTrue(); // verification is not revoked by deactivate
    }

    // ─── UpdateProfile ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateFields()
    {
        var partner = CreateValidPartner();
        var newLocation = GeoPoint.Create(59.93, 30.31);
        var newSchedule = WorkingSchedule.Create(new TimeOnly(8, 0), new TimeOnly(20, 0), "Mon-Sun");
        var newContacts = PartnerContacts.Create("+7-800-000-00-01");

        partner.UpdateProfile("New Name", "New Spec", "New Desc", "New Addr",
            newLocation, newSchedule, newContacts);

        partner.Name.ShouldBe("New Name");
        partner.Specialization.ShouldBe("New Spec");
        partner.Description.ShouldBe("New Desc");
        partner.Address.ShouldBe("New Addr");
        partner.Location.Lat.ShouldBe(59.93);
        partner.WorkingHours.WorkDays.ShouldBe("Mon-Sun");
        partner.Contacts.Phone.ShouldBe("+7-800-000-00-01");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void UpdateProfile_WithEmptyName_ShouldThrowDomainException(string name)
    {
        var partner = CreateValidPartner();

        Should.Throw<DomainException>(() =>
            partner.UpdateProfile(name, "spec", "desc", "addr",
                ValidLocation(), ValidSchedule(), ValidContacts()));
    }

    // ─── UpdateLogo ───────────────────────────────────────────────────────────

    [Fact]
    public void UpdateLogo_WithValidUrl_ShouldSetLogoUrl()
    {
        var partner = CreateValidPartner();

        partner.UpdateLogo("https://cdn.example.com/logo.png");

        partner.LogoUrl.ShouldBe("https://cdn.example.com/logo.png");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void UpdateLogo_WithEmptyUrl_ShouldThrowDomainException(string url)
    {
        var partner = CreateValidPartner();

        Should.Throw<DomainException>(() => partner.UpdateLogo(url));
    }

    // ─── SetBannerVisibility ──────────────────────────────────────────────────

    [Fact]
    public void SetBannerVisibility_ShouldUpdateShowBannersToAnonymous()
    {
        var partner = CreateValidPartner();
        partner.ShowBannersToAnonymous.ShouldBeFalse();

        partner.SetBannerVisibility(true);

        partner.ShowBannersToAnonymous.ShouldBeTrue();
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ShouldSetIsDeletedTrueAndDeactivate()
    {
        var partner = CreateValidPartner();
        partner.Verify(); // activate first

        partner.Delete();

        partner.IsDeleted.ShouldBeTrue();
        partner.IsActive.ShouldBeFalse();
    }
}
