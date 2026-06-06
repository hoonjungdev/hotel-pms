using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Rooms.Application;

public class ListRoomsHandler(HotelDbContext context)
{
    public async Task<List<RoomListItem>> HandleAsync(ListRoomsQuery query, CancellationToken cancellationToken = default)
    {
        return await context.Set<Room>()
            .Where(room => room.TenantId == query.TenantId)
            .OrderBy(room => room.Number)
            .AsNoTracking()
            .Select(room => new RoomListItem(room.Id, room.Number.Value, room.Condition.ToString()))
            .ToListAsync(cancellationToken);
    }
}
