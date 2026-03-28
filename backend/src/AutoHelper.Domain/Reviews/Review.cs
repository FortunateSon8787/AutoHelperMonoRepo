using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;

namespace AutoHelper.Domain.Reviews;

/// <summary>
/// Aggregate root representing a customer review for a partner.
/// A review can only be created when there is a verifiable interaction
/// between the customer and the partner (AI recommendation or service record).
/// </summary>
public sealed class Review : AggregateRoot<Guid>
{
    public Guid PartnerId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public ReviewBasis Basis { get; private set; }

    /// <summary>The ChatId or ServiceRecordId that proves the customer interacted with this partner.</summary>
    public Guid InteractionReferenceId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // EF Core
    private Review() { }

    /// <summary>
    /// Creates a new review. Validates all invariants before creation.
    /// </summary>
    /// <exception cref="DomainException">Thrown when any invariant is violated.</exception>
    public static Review Create(
        Guid partnerId,
        Guid customerId,
        int rating,
        string comment,
        ReviewBasis basis,
        Guid interactionReferenceId)
    {
        if (partnerId == Guid.Empty)
            throw new DomainException("Partner ID must not be empty.");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID must not be empty.");

        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Comment must not be empty.");

        if (interactionReferenceId == Guid.Empty)
            throw new DomainException("Interaction reference ID must not be empty.");

        return new Review
        {
            Id = Guid.NewGuid(),
            PartnerId = partnerId,
            CustomerId = customerId,
            Rating = rating,
            Comment = comment.Trim(),
            Basis = basis,
            InteractionReferenceId = interactionReferenceId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    /// <summary>Soft-deletes the review. Physical deletion is not permitted.</summary>
    public void Delete()
    {
        IsDeleted = true;
    }
}
