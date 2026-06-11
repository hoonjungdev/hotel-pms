using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Rooms.AddRoom;

public class AddRoomHandler(HotelDbContext context, IValidator<AddRoomCommand> validator)
{
    public async Task<AddRoomResult> HandleAsync(AddRoomCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        bool roomTypeExists = await context.Set<RoomType>()
            .AnyAsync(
                roomType => roomType.TenantId == command.TenantId && roomType.Id == command.RoomTypeId,
                cancellationToken);

        if (!roomTypeExists)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(AddRoomCommand.RoomTypeId),
                    "Room type must exist for the tenant.")
            ]);
        }

        var room = Room.Create(command.TenantId, command.RoomTypeId, RoomNumber.Create(command.Number));

        context.Set<Room>().Add(room);
        await context.SaveChangesAsync(cancellationToken);

        return new AddRoomResult(
            room.Id,
            room.RoomTypeId,
            room.Number.Value,
            room.Condition.ToString());
    }
}
