using AutoHelper.Domain.Chatbot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoHelper.Infrastructure.Persistence;

/// <summary>
/// Ensures the singleton <see cref="ChatbotConfig"/> row exists in the database.
/// Runs at startup after migrations.
/// </summary>
public static class ChatbotConfigSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var exists = await db.ChatbotConfigs.AnyAsync(c => c.Id == ChatbotConfig.SingletonId);
        if (exists)
            return;

        var config = ChatbotConfig.CreateDefault();
        db.ChatbotConfigs.Add(config);
        await db.SaveChangesAsync();

        logger.LogInformation("ChatbotConfigSeeder: default ChatbotConfig row created.");
    }
}
