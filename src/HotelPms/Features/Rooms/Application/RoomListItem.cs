using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Rooms.Application;

public sealed record RoomListItem(RoomId Id, string Number, string Condition);
