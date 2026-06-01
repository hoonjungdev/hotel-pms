using HotelPms.Features.Guests.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelPms.Features.Guests.Infrastructure.Converters;

public sealed class GuestIdConverter : ValueConverter<GuestId, Guid>
{
    public GuestIdConverter() : base(guestId => guestId.Value, value => new GuestId(value))
    {
    }
}
