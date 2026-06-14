using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Housekeeping.MarkRoomClean;

public sealed record MarkRoomCleanResult(RoomId Id, string Number, string Condition);
