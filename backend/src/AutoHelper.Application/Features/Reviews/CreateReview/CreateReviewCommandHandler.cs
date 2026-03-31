using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Reviews;
using MediatR;

namespace AutoHelper.Application.Features.Reviews.CreateReview;

public sealed class CreateReviewCommandHandler(
    IReviewRepository reviews,
    IPartnerRepository partners,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        if (currentUser.Id is not { } customerId)
            return AppErrors.Auth.NotAuthenticated;

        var partner = await partners.GetByIdAsync(request.PartnerId, ct);
        if (partner is null)
            return AppErrors.Review.PartnerNotFound;

        if (!Enum.TryParse<ReviewBasis>(request.Basis, ignoreCase: true, out var basis))
            return AppErrors.Review.InvalidBasis;

        var duplicate = await reviews.ExistsAsync(
            request.PartnerId, customerId, basis, request.InteractionReferenceId, ct);

        if (duplicate)
            return AppErrors.Review.DuplicateReview;

        var review = Review.Create(
            partnerId: request.PartnerId,
            customerId: customerId,
            rating: request.Rating,
            comment: request.Comment,
            basis: basis,
            interactionReferenceId: request.InteractionReferenceId);

        reviews.Add(review);

        var lowRatingCount = await reviews.CountLowRatingsForPartnerAsync(request.PartnerId, ct);
        partner.RecalculateFitnessFlag(lowRatingCount + (review.Rating < 3 ? 1 : 0));

        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(review.Id);
    }
}
