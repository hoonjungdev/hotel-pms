using FluentValidation;
using FluentValidation.Results;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Housekeeping.MarkRoomClean;

public class MarkRoomCleanHandler(HotelDbContext context, IValidator<MarkRoomCleanCommand> validator)
{
    public async Task<MarkRoomCleanResult?> HandleAsync(
        MarkRoomCleanCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        Room? room = await context.Set<Room>()
            .SingleOrDefaultAsync(room => room.TenantId == command.TenantId && room.Id == command.RoomId, cancellationToken);

        if (room is null)
        {
            return null;
        }

        try
        {
            room.MarkClean();
        }
        catch (InvalidOperationException exception)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(MarkRoomCleanCommand.RoomId), exception.Message)
            ]);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new MarkRoomCleanResult(
            room.Id,
            room.Number.Value,
            room.Condition.ToString());
    }
}
