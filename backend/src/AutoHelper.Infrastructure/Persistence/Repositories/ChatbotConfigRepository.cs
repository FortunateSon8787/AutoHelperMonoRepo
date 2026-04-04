using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chatbot;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class ChatbotConfigRepository(AppDbContext db) : IChatbotConfigRepository
{
    public Task<ChatbotConfig?> GetAsync(CancellationToken ct = default) =>
        db.ChatbotConfigs.FirstOrDefaultAsync(ct);

    public void Add(ChatbotConfig config) => db.ChatbotConfigs.Add(config);
}
