using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.Rooms.GetRoom;

public sealed record RoomDetails(RoomId Id, RoomTypeId RoomTypeId, string Number, string Condition);
