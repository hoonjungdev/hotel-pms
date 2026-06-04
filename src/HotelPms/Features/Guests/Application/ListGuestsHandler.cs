using HotelPms.Features.Guests.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Guests.Application;

public class ListGuestsHandler(HotelDbContext context)
{
    public async Task<List<GuestListItem>> HandleAsync(ListGuestsQuery query, CancellationToken cancellationToken = default)
    {
        return await context.Set<Guest>()
            .Where(guest => guest.TenantId == query.TenantId)
            .OrderBy(guest => guest.Name)
            .AsNoTracking()
            .Select(guest => new GuestListItem(
                guest.Id,
                guest.Name,
                guest.Email == null ? null : guest.Email.Value,
                guest.PhoneNumber == null ? null : guest.PhoneNumber.Value))
            .ToListAsync(cancellationToken);
    }
}
