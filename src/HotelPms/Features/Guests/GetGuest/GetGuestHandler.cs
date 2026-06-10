using HotelPms.Features.Guests.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Guests.GetGuest;

public class GetGuestHandler(HotelDbContext context)
{
    public async Task<GuestDetails?> HandleAsync(GetGuestQuery query, CancellationToken cancellationToken = default)
    {
        return await context.Set<Guest>()
            .Where(guest => guest.TenantId == query.TenantId && guest.Id == query.GuestId)
            .AsNoTracking()
            .Select(guest => new GuestDetails(
                guest.Id,
                guest.Name,
                guest.Email == null ? null : guest.Email.Value,
                guest.PhoneNumber == null ? null : guest.PhoneNumber.Value))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
