using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPms.Features.RoomTypes.Infrastructure;

public sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("room_types");

        builder.HasKey(roomType => roomType.Id);

        builder.Property(roomType => roomType.Id)
            .HasColumnName("id");

        builder.Property(roomType => roomType.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(roomType => roomType.Code)
            .HasColumnName("code")
            .HasMaxLength(20)
            .HasConversion(
                code => code.Value,
                value => RoomTypeCode.Create(value))
            .IsRequired();

        builder.Property(roomType => roomType.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(roomType => roomType.BaseOccupancy)
            .HasColumnName("base_occupancy")
            .IsRequired();

        builder.Property(roomType => roomType.MaxOccupancy)
            .HasColumnName("max_occupancy")
            .IsRequired();

        builder.HasIndex(roomType => new { roomType.TenantId, roomType.Code })
            .IsUnique()
            .HasDatabaseName("ix_room_types_tenant_id_code");

        builder.Ignore(roomType => roomType.DomainEvents);
    }
}
