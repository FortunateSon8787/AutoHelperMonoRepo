using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Partners.Events;

/// <summary>
/// Published when a new partner registers and is awaiting admin verification.
/// </summary>
public sealed record PartnerRegisteredEvent(Guid PartnerId, Guid AccountUserId) : IDomainEvent;
