using AutoHelper.Domain.Chatbot;
using AutoHelper.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class ChatbotConfigConfiguration : IEntityTypeConfiguration<ChatbotConfig>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<ChatbotConfig> builder)
    {
        builder.ToTable("chatbot_config");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.IsEnabled).IsRequired();
        builder.Property(c => c.MaxCharsPerField).IsRequired();

        builder.Property(c => c.TopUpPriceUsd)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(c => c.TopUpRequestCount).IsRequired();
        builder.Property(c => c.DisablePartnerSuggestionsInMode1).IsRequired();

        // DailyLimitByPlan: Dictionary<SubscriptionPlan, int> stored as jsonb
        builder.Property(c => c.DailyLimitByPlan)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(
                    v.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                    JsonOptions),
                v => JsonSerializer
                    .Deserialize<Dictionary<string, int>>(v, JsonOptions)!
                    .ToDictionary(
                        kv => Enum.Parse<SubscriptionPlan>(kv.Key),
                        kv => kv.Value))
            .IsRequired();
    }
}
