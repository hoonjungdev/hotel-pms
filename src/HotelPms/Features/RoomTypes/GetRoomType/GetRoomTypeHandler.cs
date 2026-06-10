using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.RoomTypes.GetRoomType;

public class GetRoomTypeHandler(HotelDbContext context)
{
    public async Task<RoomTypeDetails?> HandleAsync(
        GetRoomTypeQuery query,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<RoomType>()
            .Where(roomType => roomType.TenantId == query.TenantId && roomType.Id == query.RoomTypeId)
            .AsNoTracking()
            .Select(roomType => new RoomTypeDetails(
                roomType.Id,
                roomType.Code.Value,
                roomType.Name,
                roomType.BaseOccupancy,
                roomType.MaxOccupancy))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
