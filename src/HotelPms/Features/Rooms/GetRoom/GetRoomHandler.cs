using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Rooms.GetRoom;

public class GetRoomHandler(HotelDbContext context)
{
    public async Task<RoomDetails?> HandleAsync(
        GetRoomQuery query,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Room>()
            .Where(room => room.TenantId == query.TenantId && room.Id == query.RoomId)
            .AsNoTracking()
            .Select(room => new RoomDetails(
                room.Id,
                room.RoomTypeId,
                room.Number.Value,
                room.Condition.ToString()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
