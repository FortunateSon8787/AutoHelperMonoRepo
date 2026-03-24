using AutoHelper.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Vin)
            .IsRequired()
            .HasMaxLength(17);

        builder.HasIndex(v => v.Vin)
            .IsUnique();

        builder.Property(v => v.Brand)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(v => v.Year)
            .IsRequired();

        builder.Property(v => v.Color)
            .HasMaxLength(64);

        builder.Property(v => v.Mileage)
            .IsRequired();

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(v => v.OwnerId)
            .IsRequired();

        builder.HasIndex(v => v.OwnerId);
    }
}
