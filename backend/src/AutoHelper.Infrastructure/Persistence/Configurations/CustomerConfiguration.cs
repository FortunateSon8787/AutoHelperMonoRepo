using AutoHelper.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.PasswordHash)
            .HasMaxLength(512);

        builder.Property(c => c.Contacts)
            .HasMaxLength(512);

        builder.Property(c => c.SubscriptionStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.SubscriptionPlan)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired()
            .HasDefaultValue(Domain.Customers.SubscriptionPlan.None);

        builder.Property(c => c.SubscriptionStartDate);

        builder.Property(c => c.SubscriptionEndDate);

        builder.Property(c => c.RegistrationDate)
            .IsRequired();

        builder.Property(c => c.AuthProvider)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        // Google OAuth fields
        builder.Property(c => c.GoogleId)
            .HasMaxLength(256);

        builder.HasIndex(c => c.GoogleId)
            .IsUnique()
            .HasFilter("\"GoogleId\" IS NOT NULL");

        builder.Property(c => c.GoogleEmail)
            .HasMaxLength(320);

        builder.Property(c => c.GooglePicture)
            .HasMaxLength(2048);

        builder.Property(c => c.GoogleRefreshToken)
            .HasMaxLength(1024);

        builder.Property(c => c.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(c => c.AiRequestsRemaining)
            .IsRequired()
            .HasDefaultValue(0);
    }
}
