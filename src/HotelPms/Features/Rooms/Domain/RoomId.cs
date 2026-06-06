namespace HotelPms.Features.Rooms.Domain;

public readonly record struct RoomId(Guid Value)
{
    public static RoomId New() => new RoomId(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
