using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Shared.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.Guests.Domain;

public class GuestTests
{
    [Fact]
    public void Create_WithEmailOnly_ReturnsGuest()
    {
        var guest = Guest.Create(TenantId.New(), "guest", Email.Create("guest@email.com"), null);

        Assert.Equal("guest", guest.Name);
        Assert.NotNull(guest.Email);
        Assert.Null(guest.PhoneNumber);
    }

    [Fact]
    public void Create_WithPhoneOnly_ReturnsGuest()
    {
        var guest = Guest.Create(TenantId.New(), "guest", null, PhoneNumber.Create("+821012345678"));

        Assert.Equal("guest", guest.Name);
        Assert.Null(guest.Email);
        Assert.NotNull(guest.PhoneNumber);
    }

    [Fact]
    public void Create_NoContact_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Guest.Create(TenantId.New(), "guest", null, null));
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Guest.Create(TenantId.New(), "", Email.Create("guest@email.com"), null));
    }

    [Fact]
    public void Create_Always_RaisesGuestRegisteredEvent()
    {
        var guest = Guest.Create(TenantId.New(), "guest", Email.Create("guest@email.com"), null);

        IDomainEvent domainEvent = Assert.Single(guest.DomainEvents);
        GuestRegistered registered = Assert.IsType<GuestRegistered>(domainEvent);
        Assert.Equal(guest.Id, registered.GuestId);
    }

    [Fact]
    public void Create_EmptyTenantId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Guest.Create(new TenantId(Guid.Empty), "guest", Email.Create("guest@email.com"), null));
    }

    [Fact]
    public void Create_TooLongName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Guest.Create(TenantId.New(), "GuestNameIsTooLongGuestNameIsTooLongGuestNameIsTooLongGuestNameIsTooLongGuestNameIsTooLongGuestNameIsTooLong", Email.Create("guest@email.com"), null));
    }
}
