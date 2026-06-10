using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.RoomTypes.GetRoomType;

public sealed record RoomTypeDetails(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
