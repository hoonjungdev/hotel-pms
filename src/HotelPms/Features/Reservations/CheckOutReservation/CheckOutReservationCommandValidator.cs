using FluentValidation;

namespace HotelPms.Features.Reservations.CheckOutReservation;

public sealed class CheckOutReservationCommandValidator : AbstractValidator<CheckOutReservationCommand>
{
    public CheckOutReservationCommandValidator()
    {
        RuleFor(command => command.TenantId.Value)
            .NotEmpty()
            .WithMessage("Tenant ID must have value.");

        RuleFor(command => command.ReservationId.Value)
            .NotEmpty()
            .WithMessage("Reservation ID must have value.");
    }
}
