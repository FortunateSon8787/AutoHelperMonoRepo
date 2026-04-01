using AutoHelper.Domain.Admins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.HasIndex(a => a.Email)
            .IsUnique();

        builder.Property(a => a.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(a => a.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }
}
