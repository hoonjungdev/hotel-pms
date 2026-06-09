using HotelPms.Features.Rooms.Domain;

namespace HotelPms.Features.Rooms.Application;

public sealed record RoomTypeListItem(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
