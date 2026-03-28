using AutoHelper.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PartnerId)
            .IsRequired();

        builder.Property(r => r.CustomerId)
            .IsRequired();

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Basis)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(r => r.InteractionReferenceId)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.IsDeleted)
            .IsRequired();

        // One review per interaction reference per customer per partner
        builder.HasIndex(r => new { r.PartnerId, r.CustomerId, r.Basis, r.InteractionReferenceId })
            .IsUnique();

        builder.HasQueryFilter(r => !r.IsDeleted);

        // Referential integrity — no cascade delete; partner deletion is soft-delete anyway
        builder.HasOne<Domain.Partners.Partner>()
            .WithMany()
            .HasForeignKey(r => r.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
