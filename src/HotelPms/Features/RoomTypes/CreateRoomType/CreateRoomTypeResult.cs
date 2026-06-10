using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

public sealed record CreateRoomTypeResult(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
