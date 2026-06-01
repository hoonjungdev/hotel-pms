using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Guests;

[Collection(IntegrationTestCollection.Name)]
public class GuestPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GuestPersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Save_EmailOnlyGuest_RestoresGuest()
    {
        var guest = Guest.Create(
            TenantId.New(),
            "Jane Doe",
            Email.Create("Jane@EMAIL.COM"),
            null);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Guest restored = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == guest.Id);

            Assert.Equal(guest.Id, restored.Id);
            Assert.Equal(guest.TenantId, restored.TenantId);
            Assert.Equal("Jane Doe", restored.Name);
            Assert.Equal("jane@email.com", restored.Email!.Value);
            Assert.Null(restored.PhoneNumber);
        }
    }

    [Fact]
    public async Task Save_PhoneNumberOnlyGuest_RestoresGuest()
    {
        var guest = Guest.Create(
            TenantId.New(),
            "Jane Doe",
            null,
            PhoneNumber.Create("0987654321"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            await context.SaveChangesAsync();
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Guest restored = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == guest.Id);

            Assert.Equal(guest.Id, restored.Id);
            Assert.Equal(guest.TenantId, restored.TenantId);
            Assert.Equal("Jane Doe", restored.Name);
            Assert.Null(restored.Email);
            Assert.Equal("0987654321", restored.PhoneNumber!.Value);
        }
    }
}
