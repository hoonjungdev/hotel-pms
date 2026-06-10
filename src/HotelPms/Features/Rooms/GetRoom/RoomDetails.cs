using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Rooms.GetRoom;

public sealed record RoomDetails(RoomId Id, string Number, string Condition);
