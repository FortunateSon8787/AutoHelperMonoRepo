using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>Lightweight projection used for the chat list — avoids loading message content.</summary>
public sealed record ChatSummary(
    Guid Id,
    ChatMode Mode,
    ChatStatus Status,
    string Title,
    Guid? VehicleId,
    int MessageCount,
    DateTime CreatedAt);

/// <summary>
/// Data access for Chat aggregates, including eager-loaded messages.
/// </summary>
public interface IChatRepository
{
    /// <summary>Finds a chat by ID. Includes messages when <paramref name="includeMessages"/> is true.</summary>
    Task<Chat?> GetByIdAsync(Guid id, bool includeMessages, CancellationToken ct);

    /// <summary>
    /// Returns a paginated list of summary projections for all chats belonging to a customer.
    /// Does NOT load message bodies — only the message count per chat.
    /// Ordered by creation date descending.
    /// </summary>
    Task<PagedResult<ChatSummary>> GetPagedSummariesByCustomerIdAsync(
        Guid customerId, int page, int pageSize, CancellationToken ct);

    /// <summary>Adds a new chat aggregate (tracked, not yet persisted).</summary>
    void Add(Chat chat);

    /// <summary>
    /// Explicitly registers new message entities with the persistence context.
    /// Required when messages are added to a chat that was loaded from the database,
    /// because EF Core does not automatically detect additions to backing-field collections.
    /// </summary>
    void AddMessages(IEnumerable<Message> messages);
}
