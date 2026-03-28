using AutoHelper.Domain.Reviews;

namespace AutoHelper.Application.Features.Reviews.GetPartnerReviews;

public sealed record ReviewResponse(
    Guid Id,
    Guid PartnerId,
    Guid CustomerId,
    int Rating,
    string Comment,
    string Basis,
    Guid InteractionReferenceId,
    DateTime CreatedAt)
{
    public static ReviewResponse FromReview(Review r) => new(
        Id: r.Id,
        PartnerId: r.PartnerId,
        CustomerId: r.CustomerId,
        Rating: r.Rating,
        Comment: r.Comment,
        Basis: r.Basis.ToString(),
        InteractionReferenceId: r.InteractionReferenceId,
        CreatedAt: r.CreatedAt);
}
