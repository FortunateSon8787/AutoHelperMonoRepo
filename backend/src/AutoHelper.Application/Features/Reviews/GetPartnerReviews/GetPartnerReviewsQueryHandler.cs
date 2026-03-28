using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Reviews.GetPartnerReviews;

public sealed class GetPartnerReviewsQueryHandler(
    IReviewRepository reviews) : IRequestHandler<GetPartnerReviewsQuery, Result<IReadOnlyList<ReviewResponse>>>
{
    public async Task<Result<IReadOnlyList<ReviewResponse>>> Handle(
        GetPartnerReviewsQuery request,
        CancellationToken ct)
    {
        var partnerReviews = await reviews.GetByPartnerIdAsync(request.PartnerId, ct);
        var response = partnerReviews.Select(ReviewResponse.FromReview).ToList();
        return Result<IReadOnlyList<ReviewResponse>>.Success(response);
    }
}
