using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Reviews.GetPartnerReviews;

public sealed record GetPartnerReviewsQuery(Guid PartnerId) : IRequest<Result<IReadOnlyList<ReviewResponse>>>;
