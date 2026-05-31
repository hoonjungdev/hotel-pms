namespace HotelPms.Features.Guests.Domain;

public readonly record struct GuestId(Guid Value)
{
    public static GuestId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
