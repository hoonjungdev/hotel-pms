using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Rooms.AddRoom;

public sealed record AddRoomResult(RoomId Id, string Number, string Condition);
