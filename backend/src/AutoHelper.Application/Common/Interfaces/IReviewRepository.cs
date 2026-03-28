using AutoHelper.Domain.Reviews;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for the Review aggregate.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Returns true if a review already exists for the given combination,
    /// preventing duplicate reviews per interaction reference.
    /// </summary>
    Task<bool> ExistsAsync(
        Guid partnerId,
        Guid customerId,
        ReviewBasis basis,
        Guid interactionReferenceId,
        CancellationToken ct);

    /// <summary>Returns all non-deleted reviews for a given partner, ordered by creation date descending.</summary>
    Task<IReadOnlyList<Review>> GetByPartnerIdAsync(Guid partnerId, CancellationToken ct);

    /// <summary>
    /// Counts persisted reviews with rating below 3 for the given partner.
    /// Does not include reviews added to the repository but not yet saved via IUnitOfWork.
    /// The caller is responsible for adjusting the count for any in-flight reviews before
    /// passing the total to <see cref="Domain.Partners.Partner.RecalculateFitnessFlag"/>.
    /// </summary>
    Task<int> CountLowRatingsForPartnerAsync(Guid partnerId, CancellationToken ct);

    /// <summary>Adds a new review to the repository (tracked, not yet persisted).</summary>
    void Add(Review review);
}
