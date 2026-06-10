using FluentValidation;
using FluentValidation.Results;
using HotelPms.Shared.Api;

namespace HotelPms.UnitTests.Features.Shared.Api;

public class ValidationProblemMapperTests
{
    [Fact]
    public void ToErrors_ValidationFailures_ReturnsErrorsGroupedByPropertyName()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Name", "Name is required."),
            new ValidationFailure("Name", "Name is too long."),
            new ValidationFailure("Email", "Email is invalid.")
        ]);

        Dictionary<string, string[]> errors = ValidationProblemMapper.ToErrors(exception, "Guest");

        Assert.Equal(["Name is required.", "Name is too long."], errors["Name"]);
        Assert.Equal(["Email is invalid."], errors["Email"]);
    }

    [Fact]
    public void ToErrors_NoValidationFailures_ReturnsFallbackError()
    {
        var exception = new ValidationException("Guest could not be registered.");

        Dictionary<string, string[]> errors = ValidationProblemMapper.ToErrors(exception, "Guest");

        Assert.Equal(["Guest could not be registered."], errors["Guest"]);
    }
}
