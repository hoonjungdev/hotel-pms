using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPms.Features.Rooms.Infrastructure;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");

        builder.HasKey(room => room.Id);

        builder.Property(room => room.Id)
            .HasColumnName("id");

        builder.Property(room => room.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(room => room.Number)
            .HasColumnName("number")
            .HasMaxLength(20)
            .HasConversion(
                number => number.Value,
                value => RoomNumber.Create(value))
            .IsRequired();

        builder.Property(room => room.Condition)
            .HasColumnName("condition")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(room => new { room.TenantId, room.Number })
            .IsUnique()
            .HasDatabaseName("ix_rooms_tenant_id_number");

        builder.Ignore(room => room.DomainEvents);
    }
}
