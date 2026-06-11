using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.Rooms.AddRoom;

public sealed record AddRoomResult(RoomId Id, RoomTypeId RoomTypeId, string Number, string Condition);
