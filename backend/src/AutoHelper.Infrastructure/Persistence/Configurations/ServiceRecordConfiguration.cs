using AutoHelper.Domain.ServiceRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class ServiceRecordConfiguration : IEntityTypeConfiguration<ServiceRecord>
{
    public void Configure(EntityTypeBuilder<ServiceRecord> builder)
    {
        builder.ToTable("service_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.VehicleId)
            .IsRequired();

        builder.HasIndex(r => r.VehicleId);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(r => r.PerformedAt)
            .IsRequired();

        builder.Property(r => r.Cost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.ExecutorName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ExecutorContacts)
            .HasMaxLength(512);

        // Store the list of operations as a JSON column (PostgreSQL jsonb)
        builder.Property(r => r.Operations)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.DocumentUrl)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Global soft-delete filter — only non-deleted records are visible by default
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
