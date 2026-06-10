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
        await SeedRoomTypesAsync(context, cancellationToken);
        await SeedRoomsAsync(context, cancellationToken);
        await SeedGuestsAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRoomTypesAsync(HotelDbContext context, CancellationToken cancellationToken)
    {
        await AddRoomTypeIfMissingAsync(context, RoomTypeCode.Create("SGL"), "Single", 1, 1, cancellationToken);
        await AddRoomTypeIfMissingAsync(context, RoomTypeCode.Create("DBL"), "Double", 2, 3, cancellationToken);
        await AddRoomTypeIfMissingAsync(context, RoomTypeCode.Create("FAM"), "Family", 2, 5, cancellationToken);
    }

    private static async Task AddRoomTypeIfMissingAsync(
        HotelDbContext context,
        RoomTypeCode code,
        string name,
        int baseOccupancy,
        int maxOccupancy,
        CancellationToken cancellationToken)
    {
        bool exists = await context.Set<RoomType>()
            .AnyAsync(roomType => roomType.TenantId == TenantId && roomType.Code == code, cancellationToken);

        if (!exists)
        {
            context.Set<RoomType>().Add(RoomType.Create(TenantId, code, name, baseOccupancy, maxOccupancy));
        }
    }

    private static async Task SeedRoomsAsync(HotelDbContext context, CancellationToken cancellationToken)
    {
        await AddRoomIfMissingAsync(context, RoomNumber.Create("101"), cancellationToken);
        await AddRoomIfMissingAsync(context, RoomNumber.Create("102"), cancellationToken);
        await AddRoomIfMissingAsync(context, RoomNumber.Create("201"), cancellationToken);
        await AddRoomIfMissingAsync(context, RoomNumber.Create("202"), cancellationToken);
    }

    private static async Task AddRoomIfMissingAsync(
        HotelDbContext context,
        RoomNumber number,
        CancellationToken cancellationToken)
    {
        bool exists = await context.Set<Room>()
            .AnyAsync(room => room.TenantId == TenantId && room.Number == number, cancellationToken);

        if (!exists)
        {
            context.Set<Room>().Add(Room.Create(TenantId, number));
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
