using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.RegisterGuest;
using HotelPms.Infrastructure.Database;
using HotelPms.IntegrationTests.Infrastructure;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.IntegrationTests.Features.Guests.RegisterGuest;

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

        RegisterGuestResult result;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new RegisterGuestHandler(context, _validator);

            result = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Guest restored = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == result.Id);

            Assert.Equal(command.TenantId, restored.TenantId);
            Assert.Equal(command.Name, restored.Name);
            Assert.Equal("john.doe@email.com", restored.Email!.Value);
            Assert.Null(restored.PhoneNumber);
        }

        Assert.Equal(command.Name, result.Name);
        Assert.Equal("john.doe@email.com", result.Email);
        Assert.Null(result.PhoneNumber);
    }

    [Fact]
    public async Task HandleAsync_PhoneNumberOnlyCommand_PersistsGuest()
    {
        var command = new RegisterGuestCommand(TenantId.New(), "John Doe", null, "+8210-1234-5678");

        RegisterGuestResult result;

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            var handler = new RegisterGuestHandler(context, _validator);

            result = await handler.HandleAsync(command);
        }

        await using (HotelDbContext context = _fixture.CreateDbContext())
        {
            Guest restored = await context.Set<Guest>().SingleAsync(candidate => candidate.Id == result.Id);

            Assert.Equal(command.TenantId, restored.TenantId);
            Assert.Equal(command.Name, restored.Name);
            Assert.Null(restored.Email);
            Assert.Equal("+821012345678", restored.PhoneNumber!.Value);
        }

        Assert.Equal(command.Name, result.Name);
        Assert.Null(result.Email);
        Assert.Equal("+821012345678", result.PhoneNumber);
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
