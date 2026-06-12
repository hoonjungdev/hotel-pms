using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

public sealed record CreateRoomTypeResult(
    RoomTypeId Id,
    string Code,
    string Name,
    int BaseOccupancy,
    int MaxOccupancy,
    Money BaseNightlyRate);
