using AutoHelper.Domain.Chats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("chats");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerId)
            .IsRequired();

        builder.HasIndex(c => c.CustomerId);

        builder.Property(c => c.VehicleId);

        builder.Property(c => c.Mode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Allow EF Core to access the private _messages backing field
        builder.Metadata
            .FindNavigation(nameof(Chat.Messages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
