namespace HotelPms.Features.RoomTypes.CreateRoomType;

public sealed record CreateRoomTypeRequest(
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
