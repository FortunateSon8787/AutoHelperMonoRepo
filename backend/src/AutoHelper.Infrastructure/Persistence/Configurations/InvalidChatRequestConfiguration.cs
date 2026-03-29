using AutoHelper.Domain.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class InvalidChatRequestConfiguration : IEntityTypeConfiguration<InvalidChatRequest>
{
    public void Configure(EntityTypeBuilder<InvalidChatRequest> builder)
    {
        builder.ToTable("invalid_chat_requests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ChatId)
            .IsRequired();

        builder.Property(r => r.CustomerId)
            .IsRequired();

        builder.Property(r => r.UserInput)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(r => r.RejectionReason)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Index for analytics queries — find all rejections per customer or per chat.
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.ChatId);
    }
}
