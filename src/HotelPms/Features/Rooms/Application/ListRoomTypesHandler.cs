using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Rooms.Application;

public class ListRoomTypesHandler(HotelDbContext context)
{
    public async Task<List<RoomTypeListItem>> HandleAsync(
        ListRoomTypesQuery query,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<RoomType>()
            .Where(roomType => roomType.TenantId == query.TenantId)
            .OrderBy(roomType => roomType.Code)
            .AsNoTracking()
            .Select(roomType => new RoomTypeListItem(
                roomType.Id,
                roomType.Code.Value,
                roomType.Name,
                roomType.BaseOccupancy,
                roomType.MaxOccupancy))
            .ToListAsync(cancellationToken);
    }
}
