using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class InvalidChatRequestRepository(AppDbContext db) : IInvalidChatRequestRepository
{
    public void Add(InvalidChatRequest request) => db.InvalidChatRequests.Add(request);
}
