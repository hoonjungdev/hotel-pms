using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.Housekeeping.ListHousekeepingRooms;

public sealed record HousekeepingRoomListItem(RoomId Id, RoomTypeId RoomTypeId, string Number, string Condition);
