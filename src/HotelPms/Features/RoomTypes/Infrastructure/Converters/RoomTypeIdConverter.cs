using HotelPms.Features.RoomTypes.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelPms.Features.RoomTypes.Infrastructure.Converters;

public sealed class RoomTypeIdConverter : ValueConverter<RoomTypeId, Guid>
{
    public RoomTypeIdConverter()
        : base(roomTypeId => roomTypeId.Value, value => new RoomTypeId(value))
    {
    }
}
