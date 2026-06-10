using FluentValidation;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;

namespace HotelPms.Features.Guests.RegisterGuest;

public class RegisterGuestHandler(HotelDbContext context, IValidator<RegisterGuestCommand> validator)
{
    public async Task<RegisterGuestResult> HandleAsync(
        RegisterGuestCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        Email? email = string.IsNullOrWhiteSpace(command.Email) ?
            null :
            Email.Create(command.Email);

        PhoneNumber? phoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ?
            null :
            PhoneNumber.Create(command.PhoneNumber);

        var guest = Guest.Create(command.TenantId, command.Name, email, phoneNumber);

        context.Set<Guest>().Add(guest);

        await context.SaveChangesAsync(cancellationToken);

        return new RegisterGuestResult(
            guest.Id,
            guest.Name,
            guest.Email?.Value,
            guest.PhoneNumber?.Value);
    }
}
