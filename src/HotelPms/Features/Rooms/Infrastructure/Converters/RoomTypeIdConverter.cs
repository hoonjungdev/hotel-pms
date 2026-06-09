using HotelPms.Features.Rooms.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelPms.Features.Rooms.Infrastructure.Converters;

public sealed class RoomTypeIdConverter : ValueConverter<RoomTypeId, Guid>
{
    public RoomTypeIdConverter()
        : base(roomTypeId => roomTypeId.Value, value => new RoomTypeId(value))
    {
    }
}
