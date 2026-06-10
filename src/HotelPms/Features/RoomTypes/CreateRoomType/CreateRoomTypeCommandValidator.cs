using FluentValidation;

namespace HotelPms.Features.RoomTypes.CreateRoomType;

public sealed class CreateRoomTypeCommandValidator : AbstractValidator<CreateRoomTypeCommand>
{
    public CreateRoomTypeCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.Code)
            .NotEmpty()
            .WithMessage("A room type code must be provided.")
            .MaximumLength(20)
            .WithMessage("A room type code is too long.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("A room type name must be provided.")
            .MaximumLength(100)
            .WithMessage("A room type name is too long.");

        RuleFor(command => command.BaseOccupancy)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Base occupancy must be at least 1.");

        RuleFor(command => command.MaxOccupancy)
            .GreaterThanOrEqualTo(command => command.BaseOccupancy)
            .WithMessage("Max occupancy must be greater than or equal to base occupancy.");
    }
}
