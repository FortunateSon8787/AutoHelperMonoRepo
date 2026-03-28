using AutoHelper.Domain.Exceptions;
using AutoHelper.Domain.Reviews;
using Shouldly;

namespace AutoHelper.Domain.Tests.Reviews;

public class ReviewTests : TestBase
{
    private static Review CreateValidReview(
        Guid? partnerId = null,
        Guid? customerId = null,
        int rating = 4,
        string comment = "Great service!",
        ReviewBasis basis = ReviewBasis.ExecutorInServiceRecord,
        Guid? interactionReferenceId = null) =>
        Review.Create(
            partnerId: partnerId ?? Guid.NewGuid(),
            customerId: customerId ?? Guid.NewGuid(),
            rating: rating,
            comment: comment,
            basis: basis,
            interactionReferenceId: interactionReferenceId ?? Guid.NewGuid());

    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldCreateReview()
    {
        var partnerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();

        var review = Review.Create(
            partnerId: partnerId,
            customerId: customerId,
            rating: 5,
            comment: "Excellent!",
            basis: ReviewBasis.RecommendedByAI,
            interactionReferenceId: referenceId);

        review.Id.ShouldNotBe(Guid.Empty);
        review.PartnerId.ShouldBe(partnerId);
        review.CustomerId.ShouldBe(customerId);
        review.Rating.ShouldBe(5);
        review.Comment.ShouldBe("Excellent!");
        review.Basis.ShouldBe(ReviewBasis.RecommendedByAI);
        review.InteractionReferenceId.ShouldBe(referenceId);
        review.CreatedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-5));
        review.IsDeleted.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithRatingOutOfRange_ShouldThrowDomainException(int rating)
    {
        Should.Throw<DomainException>(() => CreateValidReview(rating: rating))
            .Message.ShouldContain("Rating");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyComment_ShouldThrowDomainException(string comment)
    {
        Should.Throw<DomainException>(() => CreateValidReview(comment: comment))
            .Message.ShouldContain("Comment");
    }

    [Fact]
    public void Create_WithEmptyPartnerId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() => CreateValidReview(partnerId: Guid.Empty))
            .Message.ShouldContain("Partner");
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() => CreateValidReview(customerId: Guid.Empty))
            .Message.ShouldContain("Customer");
    }

    [Fact]
    public void Create_WithEmptyInteractionReferenceId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() => CreateValidReview(interactionReferenceId: Guid.Empty))
            .Message.ShouldContain("Interaction");
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ShouldSetIsDeletedTrue()
    {
        var review = CreateValidReview();
        review.IsDeleted.ShouldBeFalse();

        review.Delete();

        review.IsDeleted.ShouldBeTrue();
    }

    // ─── Boundary rating values ────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_WithBoundaryRatingValues_ShouldSucceed(int rating)
    {
        var review = CreateValidReview(rating: rating);
        review.Rating.ShouldBe(rating);
    }
}
