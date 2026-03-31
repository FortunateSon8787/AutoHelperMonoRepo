using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Repository for the InvalidChatRequest audit log.
/// </summary>
public interface IInvalidChatRequestRepository
{
    void Add(InvalidChatRequest request);

    /// <summary>Returns the number of invalid chat requests made by a specific customer.</summary>
    Task<int> CountByCustomerAsync(Guid customerId, CancellationToken ct);
}
