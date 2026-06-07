using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Rooms.Components;

public sealed record UpdateRoomConditionRequest(RoomId RoomId, RoomCondition Condition);
