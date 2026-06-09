using FluentValidation;
using HotelPms.Features.Rooms.Domain;
using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.Rooms.Application;

public class CreateRoomTypeHandler(HotelDbContext context, IValidator<CreateRoomTypeCommand> validator)
{
    public async Task<RoomTypeId> HandleAsync(
        CreateRoomTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var code = RoomTypeCode.Create(command.Code);
        bool codeAlreadyExists = await context.Set<RoomType>()
            .AnyAsync(
                roomType => roomType.TenantId == command.TenantId && roomType.Code == code,
                cancellationToken);

        if (codeAlreadyExists)
        {
            throw new ValidationException("A room type with the same code already exists.");
        }

        var roomType = RoomType.Create(
            command.TenantId,
            code,
            command.Name,
            command.BaseOccupancy,
            command.MaxOccupancy);

        context.Set<RoomType>().Add(roomType);
        await context.SaveChangesAsync(cancellationToken);

        return roomType.Id;
    }
}
