using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.RoomTypes.ListRoomTypes;

public sealed record RoomTypeListItem(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy,
    Money BaseNightlyRate);
