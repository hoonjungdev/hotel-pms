using FluentValidation;

namespace HotelPms.Features.Reservations.CreateReservation;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.PrimaryGuestId.Value)
            .NotEmpty()
            .WithMessage("Primary guest ID must have value.");

        RuleFor(command => command.RoomTypeId.Value)
            .NotEmpty()
            .WithMessage("Room type ID must have value.");

        RuleFor(command => command.CheckOutDate)
            .GreaterThan(command => command.CheckInDate)
            .WithMessage("Check-out date must be after check-in date.");

        RuleFor(command => command.GuestCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Guest count must be at least 1.");
    }
}
