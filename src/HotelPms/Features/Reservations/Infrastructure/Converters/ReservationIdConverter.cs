using HotelPms.Features.Reservations.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelPms.Features.Reservations.Infrastructure.Converters;

public sealed class ReservationIdConverter : ValueConverter<ReservationId, Guid>
{
    public ReservationIdConverter()
        : base(reservationId => reservationId.Value, value => new ReservationId(value))
    {
    }
}
