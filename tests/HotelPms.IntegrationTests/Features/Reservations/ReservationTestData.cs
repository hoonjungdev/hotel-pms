using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Reservations;

internal static class ReservationTestData
{
    public static Guest CreateGuest(TenantId tenantId, string name = "Jane Doe")
    {
        return Guest.Create(
            tenantId,
            name,
            Email.Create($"{Guid.NewGuid():N}@example.com"),
            null);
    }

    public static RoomType CreateRoomType(
        TenantId tenantId,
        int baseOccupancy = 2,
        int maxOccupancy = 3)
    {
        string code = $"RT{Guid.NewGuid():N}"[..20];

        return RoomType.Create(
            tenantId,
            RoomTypeCode.Create(code),
            "Double",
            baseOccupancy,
            maxOccupancy);
    }

    public static Room CreateRoom(TenantId tenantId, RoomType roomType, string number)
    {
        return Room.Create(tenantId, roomType.Id, RoomNumber.Create(number));
    }

    public static Reservation CreateReservation(
        TenantId tenantId,
        Guest guest,
        RoomType roomType,
        DateOnly? checkInDate = null,
        DateOnly? checkOutDate = null,
        int guestCount = 2)
    {
        return Reservation.Create(
            tenantId,
            guest.Id,
            roomType.Id,
            new DateRange(
                checkInDate ?? new DateOnly(2026, 7, 1),
                checkOutDate ?? new DateOnly(2026, 7, 3)),
            guestCount);
    }
}
