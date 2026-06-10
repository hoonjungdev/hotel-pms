using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Rooms.UpdateRoomCondition;

public sealed record UpdateRoomConditionResult(RoomId Id, string Number, string Condition);
