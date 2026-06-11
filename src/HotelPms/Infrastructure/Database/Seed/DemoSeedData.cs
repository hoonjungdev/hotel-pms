using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Infrastructure.Database.Seed;

public static class DemoSeedData
{
    public static readonly TenantId TenantId = new(new Guid("11111111-1111-1111-1111-111111111111"));

    public static async Task SeedAsync(HotelDbContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, RoomTypeId> roomTypeIds = await SeedRoomTypesAsync(context, cancellationToken);
        await SeedRoomsAsync(context, roomTypeIds, cancellationToken);
        await SeedGuestsAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, RoomTypeId>> SeedRoomTypesAsync(
        HotelDbContext context,
        CancellationToken cancellationToken)
    {
        return new Dictionary<string, RoomTypeId>(StringComparer.OrdinalIgnoreCase)
        {
            ["SGL"] = (await GetOrAddRoomTypeAsync(
                context,
                RoomTypeCode.Create("SGL"),
                "Single",
                1,
                1,
                cancellationToken)).Id,
            ["DBL"] = (await GetOrAddRoomTypeAsync(
                context,
                RoomTypeCode.Create("DBL"),
                "Double",
                2,
                3,
                cancellationToken)).Id,
            ["FAM"] = (await GetOrAddRoomTypeAsync(
                context,
                RoomTypeCode.Create("FAM"),
                "Family",
                2,
                5,
                cancellationToken)).Id
        };
    }

    private static async Task<RoomType> GetOrAddRoomTypeAsync(
        HotelDbContext context,
        RoomTypeCode code,
        string name,
        int baseOccupancy,
        int maxOccupancy,
        CancellationToken cancellationToken)
    {
        RoomType? existing = await context.Set<RoomType>()
            .SingleOrDefaultAsync(roomType => roomType.TenantId == TenantId && roomType.Code == code, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var roomType = RoomType.Create(TenantId, code, name, baseOccupancy, maxOccupancy);
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

    private static async Task SeedGuestsAsync(HotelDbContext context, CancellationToken cancellationToken)
    {
        await AddGuestIfMissingAsync(
            context,
            "Jane Doe",
            Email.Create("jane.doe@example.com"),
            null,
            cancellationToken);

        await AddGuestIfMissingAsync(
            context,
            "Min Kim",
            null,
            PhoneNumber.Create("+8210-1234-5678"),
            cancellationToken);
    }

    private static async Task AddGuestIfMissingAsync(
        HotelDbContext context,
        string name,
        Email? email,
        PhoneNumber? phoneNumber,
        CancellationToken cancellationToken)
    {
        bool exists = await context.Set<Guest>()
            .AnyAsync(guest => guest.TenantId == TenantId && guest.Name == name, cancellationToken);

        if (!exists)
        {
            context.Set<Guest>().Add(Guest.Create(TenantId, name, email, phoneNumber));
        }
    }
}
