using System.Net;
using System.Net.Http.Json;
using HotelPms.Features.Guests;
using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Domain.ValueObjects;
using HotelPms.Features.Guests.RegisterGuest;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelPms.IntegrationTests.Features.Guests;

[Collection(IntegrationTestCollection.Name)]
public class GuestEndpointTests
{
    private const string _tenantHeaderName = "X-Tenant-Id";
    private readonly PostgreSqlFixture _fixture;

    public GuestEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Post_ValidRequest_PersistsGuest()
    {
        var tenantId = TenantId.New();
        var request = new RegisterGuestRequest("John Doe", "John.Doe@email.com", null);

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/guests", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        GuestResponse? body = await response.Content.ReadFromJsonAsync<GuestResponse>();

        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal("John Doe", body.Name);
        Assert.Equal("john.doe@email.com", body.Email);
        Assert.Null(body.PhoneNumber);

        await using HotelDbContext context = _fixture.CreateDbContext();
        Guest guest = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == new GuestId(body.Id));

        Assert.Equal(tenantId, guest.TenantId);
        Assert.Equal("John Doe", guest.Name);

        string? location = response.Headers.Location?.ToString();

        Assert.NotNull(location);
        GuestResponse? createdGuest = await client.GetFromJsonAsync<GuestResponse>(location);

        Assert.NotNull(createdGuest);
        Assert.Equal(body.Id, createdGuest.Id);
    }

    [Fact]
    public async Task Get_SameTenantGuests_ReturnsGuestsOrderedByName()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();

        var jane = Guest.Create(tenantId, "Jane", null, PhoneNumber.Create("+8210-1234-5678"));
        var john = Guest.Create(tenantId, "John", Email.Create("John.Doe@email.com"), null);
        var excluded = Guest.Create(otherTenantId, "Adam", Email.Create("Adam@email.com"), null);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().AddRange(jane, john, excluded);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        GuestResponse[]? response = await client.GetFromJsonAsync<GuestResponse[]>("/api/guests");

        Assert.NotNull(response);
        Assert.Equal(2, response.Length);
        Assert.Equal("Jane", response[0].Name);
        Assert.Equal("John", response[1].Name);
    }

    [Fact]
    public async Task Get_DifferentTenantGuest_ReturnsNotFound()
    {
        var tenantId = TenantId.New();
        var otherTenantId = TenantId.New();
        var guest = Guest.Create(otherTenantId, "Jane", null, PhoneNumber.Create("+8210-1234-5678"));

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            context.Set<Guest>().Add(guest);
            await context.SaveChangesAsync();
        }

        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = CreateClient(factory, tenantId);

        HttpResponseMessage response = await client.GetAsync($"/api/guests/{guest.Id.Value}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<HotelDbContext>>();
                    services.AddDbContext<HotelDbContext>(options => options.UseNpgsql(_fixture.ConnectionString));
                });
            });
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, TenantId tenantId)
    {
        HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        client.DefaultRequestHeaders.Add(_tenantHeaderName, tenantId.Value.ToString());

        return client;
    }
}
