using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPms.Features.Reservations.Infrastructure;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.Id)
            .HasColumnName("id");

        builder.Property(reservation => reservation.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(reservation => reservation.PrimaryGuestId)
            .HasColumnName("primary_guest_id")
            .IsRequired();

        builder.Property(reservation => reservation.RoomTypeId)
            .HasColumnName("room_type_id")
            .IsRequired();

        builder.ComplexProperty(
            reservation => reservation.StayPeriod,
            stayPeriod =>
            {
                stayPeriod.Property(period => period.Start)
                    .HasColumnName("check_in_date")
                    .IsRequired();

                stayPeriod.Property(period => period.End)
                    .HasColumnName("check_out_date")
                    .IsRequired();
            });

        builder.Property(reservation => reservation.GuestCount)
            .HasColumnName("guest_count")
            .IsRequired();

        builder.Property(reservation => reservation.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(reservation => reservation.TenantId)
            .HasDatabaseName("ix_reservations_tenant_id");

        builder.HasIndex(reservation => new { reservation.TenantId, reservation.RoomTypeId })
            .HasDatabaseName("ix_reservations_tenant_id_room_type_id");

        builder.HasOne<Guest>()
            .WithMany()
            .HasForeignKey(reservation => new { reservation.TenantId, reservation.PrimaryGuestId })
            .HasPrincipalKey(guest => new { guest.TenantId, guest.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RoomType>()
            .WithMany()
            .HasForeignKey(reservation => new { reservation.TenantId, reservation.RoomTypeId })
            .HasPrincipalKey(roomType => new { roomType.TenantId, roomType.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(reservation => reservation.DomainEvents);
    }
}
