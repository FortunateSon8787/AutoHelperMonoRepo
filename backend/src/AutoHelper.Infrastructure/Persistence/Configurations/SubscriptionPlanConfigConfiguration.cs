using AutoHelper.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionPlanConfigConfiguration : IEntityTypeConfiguration<SubscriptionPlanConfig>
{
    // Fixed IDs so seeding is idempotent across migrations
    private static readonly Guid NormalId = new("11111111-0000-0000-0000-000000000001");
    private static readonly Guid ProId    = new("11111111-0000-0000-0000-000000000002");
    private static readonly Guid MaxId    = new("11111111-0000-0000-0000-000000000003");

    public void Configure(EntityTypeBuilder<SubscriptionPlanConfig> builder)
    {
        builder.ToTable("subscription_plan_configs");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Plan)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(c => c.Plan).IsUnique();

        builder.Property(c => c.PriceUsd)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(c => c.MonthlyQuota)
            .IsRequired();

        // Seed default plan configurations
        builder.HasData(
            CreateSeed(NormalId, SubscriptionPlan.Normal, 4.99m, 10),
            CreateSeed(ProId,    SubscriptionPlan.Pro,    7.99m, 20),
            CreateSeed(MaxId,    SubscriptionPlan.Max,    12.99m, 40)
        );
    }

    private static object CreateSeed(Guid id, SubscriptionPlan plan, decimal priceUsd, int monthlyQuota) => new
    {
        Id = id,
        Plan = plan,
        PriceUsd = priceUsd,
        MonthlyQuota = monthlyQuota
    };
}
