using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain.ValueObjects;

namespace HotelPms.UnitTests.Features.Reservations.Domain;

public class ReservationAvailabilityTests
{
    [Fact]
    public void AvailableRoomCount_MoreSellableRoomsThanActiveReservations_ReturnsDifference()
    {
        var availability = new ReservationAvailability(
            RoomTypeId.New(),
            CreateStayPeriod(),
            SellableRoomCount: 3,
            ActiveReservationCount: 1);

        Assert.Equal(2, availability.AvailableRoomCount);
    }

    [Fact]
    public void AvailableRoomCount_MoreActiveReservationsThanSellableRooms_ReturnsZero()
    {
        var availability = new ReservationAvailability(
            RoomTypeId.New(),
            CreateStayPeriod(),
            SellableRoomCount: 1,
            ActiveReservationCount: 2);

        Assert.Equal(0, availability.AvailableRoomCount);
    }

    [Fact]
    public void HasAvailability_AvailableRoomCountGreaterThanZero_ReturnsTrue()
    {
        var availability = new ReservationAvailability(
            RoomTypeId.New(),
            CreateStayPeriod(),
            SellableRoomCount: 2,
            ActiveReservationCount: 1);

        Assert.True(availability.HasAvailability);
    }

    [Fact]
    public void HasAvailability_NoAvailableRooms_ReturnsFalse()
    {
        var availability = new ReservationAvailability(
            RoomTypeId.New(),
            CreateStayPeriod(),
            SellableRoomCount: 1,
            ActiveReservationCount: 1);

        Assert.False(availability.HasAvailability);
    }

    private static DateRange CreateStayPeriod()
    {
        return new DateRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3));
    }
}
