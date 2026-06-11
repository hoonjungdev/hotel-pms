namespace HotelPms.Features.Rooms;

public sealed record RoomResponse(Guid Id, Guid RoomTypeId, string Number, string Condition);
