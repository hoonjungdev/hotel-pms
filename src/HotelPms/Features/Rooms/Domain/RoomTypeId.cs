namespace HotelPms.Features.Rooms.Domain;

public readonly record struct RoomTypeId(Guid Value)
{
    public static RoomTypeId New() => new RoomTypeId(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
