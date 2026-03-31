using AutoHelper.Domain.Partners;
using AutoHelper.Domain.Reviews;

namespace AutoHelper.Application.Features.Admin.Partners;

public sealed record AdminPartnerResponse(
    Guid Id,
    string Name,
    string Type,
    string Specialization,
    string Address,
    bool IsVerified,
    bool IsActive,
    bool IsPotentiallyUnfit,
    int ReviewCount,
    double AverageRating)
{
    public static AdminPartnerResponse FromPartner(Partner partner, IReadOnlyList<Review> reviews) => new(
        Id: partner.Id,
        Name: partner.Name,
        Type: partner.Type.ToString(),
        Specialization: partner.Specialization,
        Address: partner.Address,
        IsVerified: partner.IsVerified,
        IsActive: partner.IsActive,
        IsPotentiallyUnfit: partner.IsPotentiallyUnfit,
        ReviewCount: reviews.Count,
        AverageRating: reviews.Count > 0 ? reviews.Average(r => (double)r.Rating) : 0.0);
}

public sealed record AdminPartnerListResponse(
    IReadOnlyList<AdminPartnerResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AdminReviewItemResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTime CreatedAt)
{
    public static AdminReviewItemResponse FromReview(Review review) => new(
        Id: review.Id,
        Rating: review.Rating,
        Comment: review.Comment,
        CreatedAt: review.CreatedAt);
}

public sealed record AdminPartnerDetailResponse(
    Guid Id,
    string Name,
    string Type,
    string Specialization,
    string Address,
    bool IsVerified,
    bool IsActive,
    bool IsPotentiallyUnfit,
    IReadOnlyList<AdminReviewItemResponse> Reviews)
{
    public static AdminPartnerDetailResponse FromPartner(Partner partner, IReadOnlyList<Review> reviews) => new(
        Id: partner.Id,
        Name: partner.Name,
        Type: partner.Type.ToString(),
        Specialization: partner.Specialization,
        Address: partner.Address,
        IsVerified: partner.IsVerified,
        IsActive: partner.IsActive,
        IsPotentiallyUnfit: partner.IsPotentiallyUnfit,
        Reviews: reviews.Select(AdminReviewItemResponse.FromReview).ToList());
}

public sealed record AdminUnfitPartnerResponse(
    Guid Id,
    string Name,
    string Address,
    IReadOnlyList<AdminReviewItemResponse> LowRatingReviews)
{
    public static AdminUnfitPartnerResponse FromPartner(Partner partner, IReadOnlyList<Review> lowRatingReviews) => new(
        Id: partner.Id,
        Name: partner.Name,
        Address: partner.Address,
        LowRatingReviews: lowRatingReviews.Select(AdminReviewItemResponse.FromReview).ToList());
}
