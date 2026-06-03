using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;

namespace HotelPms.Features.Guests.Application;

public class RegisterGuestHandler(HotelDbContext context)
{
    public async Task<GuestId> HandleAsync(
        RegisterGuestCommand command,
        CancellationToken cancellationToken = default)
    {
        Email? email = command.Email is null ?
            null :
            Email.Create(command.Email);

        PhoneNumber? phoneNumber = command.PhoneNumber is null ?
            null :
            PhoneNumber.Create(command.PhoneNumber);

        var guest = Guest.Create(command.TenantId, command.Name, email, phoneNumber);

        context.Set<Guest>().Add(guest);

        await context.SaveChangesAsync(cancellationToken);

        return guest.Id;
    }
}
