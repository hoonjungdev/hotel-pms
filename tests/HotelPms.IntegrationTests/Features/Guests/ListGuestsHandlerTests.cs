using HotelPms.Features.Guests.Application;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.IntegrationTests.Features.Guests;

[Collection(IntegrationTestCollection.Name)]
public class ListGuestsHandlerTests
{
    private readonly PostgreSqlFixture _fixture;

    public ListGuestsHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_SameTenantGuests_ReturnsGuestsOrderedByName()
    {
        var tenantId = TenantId.New();

        var guest1 = Guest.Create(tenantId, "Jane", null, PhoneNumber.Create("+8210-1234-5678"));
        var guest2 = Guest.Create(tenantId, "John", Email.Create("John.Doe@email.com"), null);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().AddRange(guest1, guest2);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListGuestsHandler(context);

            var query = new ListGuestsQuery(tenantId);

            List<GuestListItem> guestListItems = await handler.HandleAsync(query);

            Assert.NotNull(guestListItems);
            Assert.Equal(2, guestListItems.Count);
            Assert.Equal("Jane", guestListItems[0].Name);
            Assert.Equal("John", guestListItems[1].Name);
        }
    }

    [Fact]
    public async Task HandleAsync_DifferentTenantGuest_IsExcludedFromResults()
    {
        var tenantId1 = TenantId.New();
        var tenantId2 = TenantId.New();

        var guest1 = Guest.Create(tenantId1, "Jane", null, PhoneNumber.Create("+8210-1234-5678"));
        var guest2 = Guest.Create(tenantId2, "John", Email.Create("John.Doe@email.com"), null);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().AddRange(guest1, guest2);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new ListGuestsHandler(context);

            var query1 = new ListGuestsQuery(tenantId1);
            var query2 = new ListGuestsQuery(tenantId2);

            List<GuestListItem> guestListItems1 = await handler.HandleAsync(query1);
            List<GuestListItem> guestListItems2 = await handler.HandleAsync(query2);

            Assert.NotNull(guestListItems1);
            Assert.Single(guestListItems1);
            Assert.Equal("Jane", guestListItems1[0].Name);

            Assert.NotNull(guestListItems2);
            Assert.Single(guestListItems2);
            Assert.Equal("John", guestListItems2[0].Name);
        }
    }

    [Fact]
    public async Task HandleAsync_NoGuests_ReturnsEmptyList()
    {
        var tenantId = TenantId.New();

        await using HotelDbContext context = _fixture.CreateDbContext();

        var handler = new ListGuestsHandler(context);
        var query = new ListGuestsQuery(tenantId);

        List<GuestListItem> guestListItems = await handler.HandleAsync(query);

        Assert.NotNull(guestListItems);
        Assert.Empty(guestListItems);
    }
}
