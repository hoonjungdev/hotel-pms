using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Shared.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Guests.Domain;

public class Guest : AggregateRoot
{
    public GuestId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public string Name { get; private set; }
    public Email? Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }

    private Guest()
    {
        Name = null!;
    }

    private Guest(TenantId tenantId, string name, Email? email, PhoneNumber? phoneNumber)
    {
        Id = GuestId.New();
        TenantId = tenantId;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static Guest Create(TenantId tenantId, string name, Email? email, PhoneNumber? phoneNumber)
    {
        EnsureValidName(name);
        EnsureAtLeastOneContact(email, phoneNumber);

        var guest = new Guest(tenantId, name, email, phoneNumber);
        guest.RaiseDomainEvent(new GuestRegistered(guest.Id, guest.TenantId));
        return guest;
    }

    private static void EnsureValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A name must be provided.", nameof(name));
        }
    }

    private static void EnsureAtLeastOneContact(Email? email, PhoneNumber? phoneNumber)
    {
        if (email == null && phoneNumber == null)
        {
            throw new ArgumentException("At least one of email or phone must be provided.");
        }
    }
}
