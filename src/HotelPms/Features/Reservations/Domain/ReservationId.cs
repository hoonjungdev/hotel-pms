namespace HotelPms.Features.Reservations.Domain;

public readonly record struct ReservationId(Guid Value)
{
    public static ReservationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
