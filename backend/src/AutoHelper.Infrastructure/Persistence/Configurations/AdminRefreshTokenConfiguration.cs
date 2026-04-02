using AutoHelper.Domain.Admins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class AdminRefreshTokenConfiguration : IEntityTypeConfiguration<AdminRefreshToken>
{
    public void Configure(EntityTypeBuilder<AdminRefreshToken> builder)
    {
        builder.ToTable("admin_refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.AdminUserId)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        // FK to admin_users — refresh tokens are deleted when admin is deleted
        builder.HasOne<AdminUser>()
            .WithMany()
            .HasForeignKey(rt => rt.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
