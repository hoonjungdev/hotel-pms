using FluentValidation;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

public class CreateRoomTypeHandler(HotelDbContext context, IValidator<CreateRoomTypeCommand> validator)
{
    public async Task<CreateRoomTypeResult> HandleAsync(
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

        var baseNightlyRate = new Money(
            command.BaseNightlyRateAmount,
            Enum.Parse<Currency>(command.BaseNightlyRateCurrency, ignoreCase: true));

        var roomType = RoomType.Create(
            command.TenantId,
            code,
            command.Name,
            command.BaseOccupancy,
            command.MaxOccupancy,
            baseNightlyRate);

        context.Set<RoomType>().Add(roomType);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateRoomTypeResult(
            roomType.Id,
            roomType.Code.Value,
            roomType.Name,
            roomType.BaseOccupancy,
            roomType.MaxOccupancy,
            roomType.BaseNightlyRate);
    }
}
