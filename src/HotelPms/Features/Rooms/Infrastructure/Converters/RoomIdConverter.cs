using HotelPms.Features.Rooms.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelPms.Features.Rooms.Infrastructure.Converters;

public sealed class RoomIdConverter : ValueConverter<RoomId, Guid>
{
    public RoomIdConverter()
        : base(roomId => roomId.Value, value => new RoomId(value))
    {
    }
}
