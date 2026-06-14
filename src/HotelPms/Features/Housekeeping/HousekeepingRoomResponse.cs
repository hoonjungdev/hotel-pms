namespace HotelPms.Features.Housekeeping;

public sealed record HousekeepingRoomResponse(Guid Id, Guid RoomTypeId, string Number, string Condition);
