using FluentValidation;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Rooms.Application;

public class UpdateRoomConditionHandler(HotelDbContext context, IValidator<UpdateRoomConditionCommand> validator)
{
    public async Task HandleAsync(UpdateRoomConditionCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        Room? room = await context.Set<Room>()
            .SingleOrDefaultAsync(room => room.TenantId == command.TenantId && room.Id == command.RoomId, cancellationToken);

        if (room is null)
        {
            return;
        }

        switch (command.Condition)
        {
            case RoomCondition.Clean:
                room.MarkClean();
                break;
            case RoomCondition.Dirty:
                room.MarkDirty();
                break;
            case RoomCondition.OutOfService:
                room.TakeOutOfService();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command.Condition), command.Condition, "Unsupported room condition.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
