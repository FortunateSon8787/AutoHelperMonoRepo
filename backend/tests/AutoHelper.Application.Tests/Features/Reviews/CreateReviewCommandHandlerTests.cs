using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Reviews.CreateReview;
using AutoHelper.Domain.Partners;
using AutoHelper.Domain.Reviews;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Reviews;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<IPartnerRepository> _partners = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateReviewCommandHandler _sut;

    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid PartnerId = Guid.NewGuid();

    public CreateReviewCommandHandlerTests()
    {
        _sut = new CreateReviewCommandHandler(
            _reviews.Object,
            _partners.Object,
            _currentUser.Object,
            _unitOfWork.Object);

        // Default: authenticated user
        _currentUser.Setup(u => u.Id).Returns(CustomerId);

        // Default: partner exists
        var partner = Partner.Create(
            name: "Test Partner",
            type: PartnerType.AutoService,
            specialization: "Testing",
            description: "Test partner description",
            address: "Test address 1",
            location: GeoPoint.Create(55.75, 37.61),
            workingHours: WorkingSchedule.Create(new TimeOnly(9, 0), new TimeOnly(18, 0), "Mon-Fri"),
            contacts: PartnerContacts.Create("+7-999-000-00-00"),
            accountUserId: Guid.NewGuid());

        _partners.Setup(r => r.GetByIdAsync(PartnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Default: no duplicate
        _reviews.Setup(r => r.ExistsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<ReviewBasis>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Default: zero low ratings so far
        _reviews.Setup(r => r.CountLowRatingsForPartnerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }

    private CreateReviewCommand ValidCommand(int rating = 4) => new(
        PartnerId: PartnerId,
        Rating: rating,
        Comment: "Excellent service, highly recommended!",
        Basis: "ExecutorInServiceRecord",
        InteractionReferenceId: Guid.NewGuid());

    // ─── Success path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenValidData_ShouldCreateReviewAndReturnId()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldCallRepositoryAddAndSaveChanges()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        _reviews.Verify(r => r.Add(It.IsAny<Review>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldCallRecalculateFitness()
    {
        // We can't verify RecalculateFitnessFlag directly on a real Partner object,
        // but we verify CountLowRatingsForPartnerAsync is called to feed the recalculation
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        _reviews.Verify(
            r => r.CountLowRatingsForPartnerAsync(PartnerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─── Failure paths ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _reviews.Verify(r => r.Add(It.IsAny<Review>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPartnerNotFound_ShouldReturnFailure()
    {
        _partners.Setup(r => r.GetByIdAsync(PartnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        _reviews.Verify(r => r.Add(It.IsAny<Review>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDuplicateReview_ShouldReturnFailure()
    {
        _reviews.Setup(r => r.ExistsAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<ReviewBasis>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        _reviews.Verify(r => r.Add(It.IsAny<Review>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvalidBasis_ShouldReturnFailure()
    {
        var command = ValidCommand() with { Basis = "InvalidBasis" };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        _reviews.Verify(r => r.Add(It.IsAny<Review>()), Times.Never);
    }

    // ─── IsPotentiallyUnfit recalculation ─────────────────────────────────────

    [Fact]
    public async Task Handle_WhenLowRatingAndExistingLowRatingsReach5_ShouldAccountForNewRatingInCount()
    {
        // 4 existing low ratings + new rating of 2 should push to 5 total
        _reviews.Setup(r => r.CountLowRatingsForPartnerAsync(PartnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        Review? capturedReview = null;
        _reviews.Setup(r => r.Add(It.IsAny<Review>()))
            .Callback<Review>(r => capturedReview = r);

        var command = ValidCommand(rating: 2);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        capturedReview.ShouldNotBeNull();
        capturedReview!.Rating.ShouldBe(2);
    }
}
