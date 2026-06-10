using HotelPms.Features.RoomTypes.Domain;

namespace HotelPms.Features.RoomTypes.ListRoomTypes;

public sealed record RoomTypeListItem(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
