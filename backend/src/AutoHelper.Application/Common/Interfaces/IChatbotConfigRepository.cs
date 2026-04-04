using AutoHelper.Domain.Chatbot;

namespace AutoHelper.Application.Common.Interfaces;

public interface IChatbotConfigRepository
{
    Task<ChatbotConfig?> GetAsync(CancellationToken ct = default);
    void Add(ChatbotConfig config);
}
