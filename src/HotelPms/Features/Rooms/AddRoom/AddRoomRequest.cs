namespace HotelPms.Features.Rooms.AddRoom;

public sealed record AddRoomRequest(Guid RoomTypeId, string Number);
