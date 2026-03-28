using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Reviews;
using AutoHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository(AppDbContext db) : IReviewRepository
{
    public Task<bool> ExistsAsync(
        Guid partnerId,
        Guid customerId,
        ReviewBasis basis,
        Guid interactionReferenceId,
        CancellationToken ct) =>
        db.Reviews.AnyAsync(r =>
            r.PartnerId == partnerId &&
            r.CustomerId == customerId &&
            r.Basis == basis &&
            r.InteractionReferenceId == interactionReferenceId, ct);

    public Task<IReadOnlyList<Review>> GetByPartnerIdAsync(Guid partnerId, CancellationToken ct) =>
        db.Reviews
            .Where(r => r.PartnerId == partnerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Review>)t.Result, ct);

    public Task<int> CountLowRatingsForPartnerAsync(Guid partnerId, CancellationToken ct) =>
        db.Reviews.CountAsync(r => r.PartnerId == partnerId && r.Rating < 3, ct);

    public void Add(Review review) =>
        db.Reviews.Add(review);
}
