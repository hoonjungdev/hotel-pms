using FluentValidation;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;

namespace HotelPms.Features.Rooms.AddRoom;

public class AddRoomHandler(HotelDbContext context, IValidator<AddRoomCommand> validator)
{
    public async Task<AddRoomResult> HandleAsync(AddRoomCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var room = Room.Create(command.TenantId, RoomNumber.Create(command.Number));

        context.Set<Room>().Add(room);
        await context.SaveChangesAsync(cancellationToken);

        return new AddRoomResult(
            room.Id,
            room.Number.Value,
            room.Condition.ToString());
    }
}
