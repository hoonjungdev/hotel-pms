using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.RoomTypes.GetRoomType;

public sealed record RoomTypeDetails(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy,
    Money BaseNightlyRate);
