using HotelPms.Features.Guests.Application;
using HotelPms.Features.Guests.Domain;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Guests;

[Collection(IntegrationTestCollection.Name)]
public class RegisterGuestHandlerTests
{
    private readonly PostgreSqlFixture _fixture;
    private readonly RegisterGuestCommandValidator _validator = new();

    public RegisterGuestHandlerTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HandleAsync_EmailOnlyCommand_PersistsGuest()
    {
        var command = new RegisterGuestCommand(TenantId.New(), "John Doe", "John.Doe@email.com", null);

        GuestId guestId;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new RegisterGuestHandler(context, _validator);

            guestId = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Guest restored = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == guestId);

            Assert.Equal(command.TenantId, restored.TenantId);
            Assert.Equal(command.Name, restored.Name);
            Assert.Equal("john.doe@email.com", restored.Email!.Value);
            Assert.Null(restored.PhoneNumber);
        }
    }

    [Fact]
    public async Task HandleAsync_PhoneNumberOnlyCommand_PersistsGuest()
    {
        var command = new RegisterGuestCommand(TenantId.New(), "John Doe", null, "+8210-1234-5678");

        GuestId guestId;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new RegisterGuestHandler(context, _validator);

            guestId = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Guest restored = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == guestId);

            Assert.Equal(command.TenantId, restored.TenantId);
            Assert.Equal(command.Name, restored.Name);
            Assert.Null(restored.Email);
            Assert.Equal("+821012345678", restored.PhoneNumber!.Value);
        }
    }

    [Fact]
    public async Task HandleAsync_NoContact_ThrowsValidationException()
    {
        var command = new RegisterGuestCommand(TenantId.New(), "John Doe", null, null);

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new RegisterGuestHandler(context, _validator);

            await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await handler.HandleAsync(command));
        }
    }
}
