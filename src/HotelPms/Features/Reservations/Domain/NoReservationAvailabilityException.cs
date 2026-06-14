using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.Reservations.Domain;

public sealed class NoReservationAvailabilityException : InvalidOperationException
{
    public NoReservationAvailabilityException(RoomTypeId roomTypeId, DateRange stayPeriod)
        : base("No rooms are available for the requested stay period.")
    {
        RoomTypeId = roomTypeId;
        StayPeriod = stayPeriod;
    }

    public RoomTypeId RoomTypeId { get; }
    public DateRange StayPeriod { get; }
}
