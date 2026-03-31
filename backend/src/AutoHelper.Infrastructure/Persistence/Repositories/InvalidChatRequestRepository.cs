using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class InvalidChatRequestRepository(AppDbContext db) : IInvalidChatRequestRepository
{
    public void Add(InvalidChatRequest request) => db.InvalidChatRequests.Add(request);

    public Task<int> CountByCustomerAsync(Guid customerId, CancellationToken ct) =>
        db.InvalidChatRequests.CountAsync(r => r.CustomerId == customerId, ct);
}
