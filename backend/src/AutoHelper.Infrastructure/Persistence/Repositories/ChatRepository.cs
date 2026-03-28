using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class ChatRepository(AppDbContext context) : IChatRepository
{
    public Task<Chat?> GetByIdAsync(Guid id, bool includeMessages, CancellationToken ct)
    {
        var query = context.Chats.AsQueryable();

        if (includeMessages)
            query = query.Include(c => c.Messages);

        return query.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<ChatSummary>> GetSummariesByCustomerIdAsync(Guid customerId, CancellationToken ct)
    {
        return await context.Chats
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChatSummary(
                c.Id,
                c.Mode,
                c.Title,
                c.VehicleId,
                c.Messages.Count,
                c.CreatedAt))
            .ToListAsync(ct);
    }

    public void Add(Chat chat) =>
        context.Chats.Add(chat);
}
