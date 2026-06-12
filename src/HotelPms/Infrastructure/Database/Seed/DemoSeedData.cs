using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Features.Pricing.Domain;
using HotelPms.Features.Reservations.Domain;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Infrastructure.Database.Seed;

public static class DemoSeedData
{
    public static readonly TenantId TenantId = new(new Guid("11111111-1111-1111-1111-111111111111"));

    public static async Task SeedAsync(HotelDbContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, RoomType> roomTypes = await SeedRoomTypesAsync(context, cancellationToken);
        Dictionary<string, Guest> guests = await SeedGuestsAsync(context, cancellationToken);

        await SeedRoomsAsync(
            context,
            roomTypes.ToDictionary(pair => pair.Key, pair => pair.Value.Id, StringComparer.OrdinalIgnoreCase),
            cancellationToken);
        await SeedReservationsAsync(context, roomTypes, guests, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, RoomType>> SeedRoomTypesAsync(
        HotelDbContext context,
        CancellationToken cancellationToken)
    {
        return new Dictionary<string, RoomType>(StringComparer.OrdinalIgnoreCase)
        {
            ["SGL"] = (await GetOrAddRoomTypeAsync(
                context,
                RoomTypeCode.Create("SGL"),
                "Single",
                1,
                1,
                new Money(80_000, Currency.KRW),
                cancellationToken)),
            ["DBL"] = (await GetOrAddRoomTypeAsync(
                context,
                RoomTypeCode.Create("DBL"),
                "Double",
                2,
                3,
                new Money(120_000, Currency.KRW),
                cancellationToken)),
            ["FAM"] = (await GetOrAddRoomTypeAsync(
                context,
                RoomTypeCode.Create("FAM"),
                "Family",
                2,
                5,
                new Money(180_000, Currency.KRW),
                cancellationToken))
        };
    }

    private static async Task<RoomType> GetOrAddRoomTypeAsync(
        HotelDbContext context,
        RoomTypeCode code,
        string name,
        int baseOccupancy,
        int maxOccupancy,
        Money baseNightlyRate,
        CancellationToken cancellationToken)
    {
        RoomType? existing = await context.Set<RoomType>()
            .SingleOrDefaultAsync(roomType => roomType.TenantId == TenantId && roomType.Code == code, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var roomType = RoomType.Create(TenantId, code, name, baseOccupancy, maxOccupancy, baseNightlyRate);
        context.Set<RoomType>().Add(roomType);

        return roomType;
    }

    private static async Task SeedRoomsAsync(
        HotelDbContext context,
        IReadOnlyDictionary<string, RoomTypeId> roomTypeIds,
        CancellationToken cancellationToken)
    {
        await AddRoomIfMissingAsync(context, roomTypeIds["SGL"], RoomNumber.Create("101"), cancellationToken);
        await AddRoomIfMissingAsync(context, roomTypeIds["SGL"], RoomNumber.Create("102"), cancellationToken);
        await AddRoomIfMissingAsync(context, roomTypeIds["DBL"], RoomNumber.Create("201"), cancellationToken);
        await AddRoomIfMissingAsync(context, roomTypeIds["FAM"], RoomNumber.Create("202"), cancellationToken);
    }

    private static async Task AddRoomIfMissingAsync(
        HotelDbContext context,
        RoomTypeId roomTypeId,
        RoomNumber number,
        CancellationToken cancellationToken)
    {
        bool exists = await context.Set<Room>()
            .AnyAsync(room => room.TenantId == TenantId && room.Number == number, cancellationToken);

        if (!exists)
        {
            context.Set<Room>().Add(Room.Create(TenantId, roomTypeId, number));
        }
    }

    private static async Task<Dictionary<string, Guest>> SeedGuestsAsync(
        HotelDbContext context,
        CancellationToken cancellationToken)
    {
        return new Dictionary<string, Guest>(StringComparer.OrdinalIgnoreCase)
        {
            ["Jane Doe"] = await GetOrAddGuestAsync(
                context,
                "Jane Doe",
                Email.Create("jane.doe@example.com"),
                null,
                cancellationToken),
            ["Min Kim"] = await GetOrAddGuestAsync(
                context,
                "Min Kim",
                null,
                PhoneNumber.Create("+8210-1234-5678"),
                cancellationToken)
        };
    }

    private static async Task<Guest> GetOrAddGuestAsync(
        HotelDbContext context,
        string name,
        Email? email,
        PhoneNumber? phoneNumber,
        CancellationToken cancellationToken)
    {
        Guest? existing = await context.Set<Guest>()
            .SingleOrDefaultAsync(guest => guest.TenantId == TenantId && guest.Name == name, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var guest = Guest.Create(TenantId, name, email, phoneNumber);
        context.Set<Guest>().Add(guest);

        return guest;
    }

    private static async Task SeedReservationsAsync(
        HotelDbContext context,
        IReadOnlyDictionary<string, RoomType> roomTypes,
        IReadOnlyDictionary<string, Guest> guests,
        CancellationToken cancellationToken)
    {
        var stayPeriod = new DateRange(new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 12));
        RoomType roomType = roomTypes["SGL"];
        Guest guest = guests["Jane Doe"];

        bool exists = await context.Set<Reservation>()
            .AnyAsync(
                reservation =>
                    reservation.TenantId == TenantId &&
                    reservation.PrimaryGuestId == guest.Id &&
                    reservation.RoomTypeId == roomType.Id &&
                    reservation.StayPeriod.Start == stayPeriod.Start &&
                    reservation.StayPeriod.End == stayPeriod.End,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var reservation = Reservation.Create(
            TenantId,
            guest.Id,
            roomType.Id,
            stayPeriod,
            guestCount: 1,
            PriceCalculator.CalculateStayTotal(roomType.BaseNightlyRate, stayPeriod));
        reservation.Confirm();

        context.Set<Reservation>().Add(reservation);
    }
}
