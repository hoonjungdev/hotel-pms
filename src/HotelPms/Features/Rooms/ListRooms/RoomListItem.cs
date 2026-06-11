using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.Rooms.ListRooms;

public sealed record RoomListItem(RoomId Id, RoomTypeId RoomTypeId, string Number, string Condition);
