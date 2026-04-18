using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class ChatRepository(AppDbContext context) : IChatRepository
{
    public Task<Chat?> GetByIdAsync(Guid id, bool includeMessages, CancellationToken ct)
    {
        var query = context.Chats
            .Where(c => c.Id == id && !c.IsDeleted);

        if (includeMessages)
            query = query.Include(c => c.Messages);

        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ChatSummary>> GetPagedSummariesByCustomerIdAsync(
        Guid customerId, int page, int pageSize, CancellationToken ct)
    {
        var baseQuery = context.Chats
            .Where(c => c.CustomerId == customerId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ChatSummary(
                c.Id,
                c.Mode,
                c.Status,
                c.Title,
                c.VehicleId,
                c.Messages.Count,
                c.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ChatSummary>(items, totalCount, page, pageSize);
    }

    public void Add(Chat chat) =>
        context.Chats.Add(chat);

    public void AddMessages(IEnumerable<Message> messages) =>
        context.Messages.AddRange(messages);
}
