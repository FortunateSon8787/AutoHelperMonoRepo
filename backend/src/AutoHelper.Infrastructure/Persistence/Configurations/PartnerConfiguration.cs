using AutoHelper.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoHelper.Infrastructure.Persistence.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(p => p.Specialization)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(512);

        // GeoPoint — owned (two columns: lat, lng)
        builder.OwnsOne(p => p.Location, geo =>
        {
            geo.Property(g => g.Lat)
                .HasColumnName("location_lat")
                .IsRequired();

            geo.Property(g => g.Lng)
                .HasColumnName("location_lng")
                .IsRequired();
        });

        // WorkingSchedule — owned (three columns)
        builder.OwnsOne(p => p.WorkingHours, wh =>
        {
            wh.Property(w => w.OpenFrom)
                .HasColumnName("working_open_from")
                .IsRequired();

            wh.Property(w => w.OpenTo)
                .HasColumnName("working_open_to")
                .IsRequired();

            wh.Property(w => w.WorkDays)
                .HasColumnName("working_days")
                .HasMaxLength(128)
                .IsRequired();
        });

        // PartnerContacts — owned
        builder.OwnsOne(p => p.Contacts, c =>
        {
            c.Property(x => x.Phone)
                .HasColumnName("contacts_phone")
                .HasMaxLength(64)
                .IsRequired();

            c.Property(x => x.Website)
                .HasColumnName("contacts_website")
                .HasMaxLength(2048);

            c.Property(x => x.MessengerLinks)
                .HasColumnName("contacts_messenger_links")
                .HasMaxLength(1024);
        });

        builder.Property(p => p.LogoUrl)
            .HasMaxLength(2048);

        builder.Property(p => p.IsVerified)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.IsPotentiallyUnfit)
            .IsRequired();

        builder.Property(p => p.ShowBannersToAnonymous)
            .IsRequired();

        builder.Property(p => p.AccountUserId)
            .IsRequired();

        builder.HasIndex(p => p.AccountUserId)
            .IsUnique(); // one partner profile per user account

        builder.Property(p => p.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
