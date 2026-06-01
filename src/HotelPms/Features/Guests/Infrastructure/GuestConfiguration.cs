using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPms.Features.Guests.Infrastructure;

public sealed class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("guests");

        builder.HasKey(guest => guest.Id);

        builder.Property(guest => guest.Id)
            .HasColumnName("id");

        builder.Property(guest => guest.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(guest => guest.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(guest => guest.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .HasConversion(
                email => email!.Value,
                value => Email.Create(value));

        builder.Property(guest => guest.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(16)
            .HasConversion(
                phoneNumber => phoneNumber!.Value,
                value => PhoneNumber.Create(value));

        builder.HasIndex(guest => guest.TenantId)
            .HasDatabaseName("ix_guests_tenant_id");

        builder.Ignore(guest => guest.DomainEvents);
    }
}
