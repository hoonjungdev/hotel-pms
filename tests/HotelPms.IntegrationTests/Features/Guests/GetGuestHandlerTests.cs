using HotelPms.Features.Guests.Application;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Guests;

[Collection(IntegrationTestCollection.Name)]
public class GetGuestHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public GetGuestHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_ExistingGuest_ReturnsGuestDetails()
    {
        var tenantId = TenantId.New();

        var guest = Guest.Create(tenantId, "Jane", null, PhoneNumber.Create("+8210-1234-5678"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);

            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var query = new GetGuestQuery(tenantId, guest.Id);

            var handler = new GetGuestHandler(context);
            GuestDetails? details = await handler.HandleAsync(query);

            Assert.NotNull(details);
            Assert.Equal(guest.Id, details.Id);
            Assert.Equal("Jane", details.Name);
            Assert.Null(details.Email);
            Assert.Equal("+821012345678", details.PhoneNumber);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantGuest_ReturnsNull()
    {
        var tenantId1 = TenantId.New();
        var tenantId2 = TenantId.New();

        var guest = Guest.Create(tenantId1, "Jane", null, PhoneNumber.Create("+8210-1234-5678"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);

            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var query = new GetGuestQuery(tenantId2, guest.Id);

            var handler = new GetGuestHandler(context);
            GuestDetails? details = await handler.HandleAsync(query);

            Assert.Null(details);
        }
    }

    [Fact]
    public async Task HandleAsync_MissingGuest_ReturnsNull()
    {
        var tenantId = TenantId.New();
        var guestId = GuestId.New();

        await using HotelDbContext context = _fixture.CreateDbContext();

        var query = new GetGuestQuery(tenantId, guestId);

        var handler = new GetGuestHandler(context);
        GuestDetails? details = await handler.HandleAsync(query);

        Assert.Null(details);
    }
}
