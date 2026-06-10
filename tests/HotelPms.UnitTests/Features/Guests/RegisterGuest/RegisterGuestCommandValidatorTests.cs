using FluentValidation.Results;
using HotelPms.Features.Guests.RegisterGuest;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.UnitTests.Features.Guests.RegisterGuest;

public class RegisterGuestCommandValidatorTests
{
    private readonly RegisterGuestCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidEmailOnlyCommand_ReturnsNoErrors()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            "john.doe@email.com",
            null);

        ValidationResult result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidPhoneNumberOnlyCommand_ReturnsNoErrors()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            null,
            "8210-1234-1234");

        ValidationResult result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NoContact_ReturnsContactError()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            null,
            null);

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorMessage == "Either email or phone number must be provided.");
    }

    [Fact]
    public void Validate_BlankName_ReturnsNameError()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "",
            "john.doe@email.com",
            null);

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NameLongerThan100Characters_ReturnsNameError()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            new string('a', 101),
            "john.doe@email.com",
            null);

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsEmailError()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            "not-an-email",
            null);

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
    }

    [Fact]
    public void Validate_InvalidPhoneNumber_ReturnsPhoneNumberError()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            null,
            "1234");

        ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void Validate_ValidEmailWithBlankPhoneNumber_ReturnsNoErrors()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            "john.doe@email.com",
            "");

        ValidationResult result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidPhoneNumberWithBlankEmail_ReturnsNoErrors()
    {
        var command = new RegisterGuestCommand(
            TenantId.New(),
            "John Doe",
            "",
            "+82010-1234-1234");

        ValidationResult result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
