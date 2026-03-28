using AutoHelper.Domain.AdCampaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class AdCampaignConfiguration : IEntityTypeConfiguration<AdCampaign>
{
    public void Configure(EntityTypeBuilder<AdCampaign> builder)
    {
        builder.ToTable("ad_campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PartnerId)
            .IsRequired();

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.TargetCategory)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(c => c.StartsAt)
            .IsRequired();

        builder.Property(c => c.EndsAt)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.ShowToAnonymous)
            .IsRequired();

        // AdStats — owned value object (two columns)
        builder.OwnsOne(c => c.Stats, stats =>
        {
            stats.Property(s => s.Impressions)
                .HasColumnName("stats_impressions")
                .IsRequired();

            stats.Property(s => s.Clicks)
                .HasColumnName("stats_clicks")
                .IsRequired();
        });

        builder.Property(c => c.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(c => !c.IsDeleted);

        // FK to partners — no cascade; partner deletion is soft-delete anyway
        builder.HasOne<Domain.Partners.Partner>()
            .WithMany()
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for partner-specific queries
        builder.HasIndex(c => c.PartnerId);

        // Index for display queries (active + schedule)
        builder.HasIndex(c => new { c.IsActive, c.StartsAt, c.EndsAt });
    }
}
