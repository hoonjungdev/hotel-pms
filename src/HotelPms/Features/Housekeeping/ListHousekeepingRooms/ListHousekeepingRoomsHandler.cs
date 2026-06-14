using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Housekeeping.ListHousekeepingRooms;

public class ListHousekeepingRoomsHandler(HotelDbContext context)
{
    public async Task<List<HousekeepingRoomListItem>> HandleAsync(
        ListHousekeepingRoomsQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Room> rooms = context.Set<Room>()
            .Where(room => room.TenantId == query.TenantId);

        if (query.Condition is not null)
        {
            rooms = rooms.Where(room => room.Condition == query.Condition);
        }

        return await rooms
            .OrderBy(room => room.Condition == RoomCondition.Dirty ? 0 :
                room.Condition == RoomCondition.OutOfService ? 1 : 2)
            .ThenBy(room => room.Number)
            .AsNoTracking()
            .Select(room => new HousekeepingRoomListItem(
                room.Id,
                room.RoomTypeId,
                room.Number.Value,
                room.Condition.ToString()))
            .ToListAsync(cancellationToken);
    }
}
