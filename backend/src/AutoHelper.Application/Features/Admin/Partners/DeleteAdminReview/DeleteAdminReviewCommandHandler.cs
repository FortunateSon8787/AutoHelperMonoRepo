using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.DeleteAdminReview;

public sealed class DeleteAdminReviewCommandHandler(
    IReviewRepository reviews,
    IPartnerRepository partners,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminReviewCommand, Result>
{
    public async Task<Result> Handle(DeleteAdminReviewCommand request, CancellationToken ct)
    {
        var review = await reviews.GetByIdAsync(request.ReviewId, ct);
        if (review is null)
            return Result.Failure(AppErrors.Admin.ReviewNotFound);

        var partner = await partners.GetByIdAsync(review.PartnerId, ct);

        // Count low-ratings BEFORE marking deleted so the query filter still includes this review.
        // Then adjust by -1 if the deleted review itself was low-rated.
        if (partner is not null)
        {
            var lowRatingCount = await reviews.CountLowRatingsForPartnerAsync(partner.Id, ct);
            var adjustment = review.Rating < 3 ? -1 : 0;
            partner.RecalculateFitnessFlag(lowRatingCount + adjustment);
        }

        review.Delete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
