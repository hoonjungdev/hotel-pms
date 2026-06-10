namespace HotelPms.Features.RoomTypes;

public sealed record RoomTypeResponse(
    Guid Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy);
