using AutoHelper.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.CustomerId)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        // FK to customers — refresh tokens are deleted when the customer is deleted
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(rt => rt.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
