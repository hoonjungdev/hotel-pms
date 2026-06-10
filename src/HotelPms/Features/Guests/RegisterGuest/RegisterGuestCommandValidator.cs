using FluentValidation;
using HotelPms.Features.Guests.Domain.ValueObjects;

namespace HotelPms.Features.Guests.RegisterGuest;

public sealed class RegisterGuestCommandValidator : AbstractValidator<RegisterGuestCommand>
{
    public RegisterGuestCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Email)
            .Must(BeValidEmail)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        RuleFor(command => command.PhoneNumber)
            .Must(BeValidPhoneNumber)
            .When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber));

        RuleFor(command => command)
            .Must(HaveAtLeastOneContact)
            .WithMessage("Either email or phone number must be provided.");
    }

    private static bool BeValidEmail(string? email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            Email.Create(email);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    private static bool BeValidPhoneNumber(string? phoneNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            PhoneNumber.Create(phoneNumber);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    private static bool HaveAtLeastOneContact(RegisterGuestCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Email)
               || !string.IsNullOrWhiteSpace(command.PhoneNumber);
    }
}
