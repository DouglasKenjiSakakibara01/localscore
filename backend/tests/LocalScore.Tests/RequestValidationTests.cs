using System.ComponentModel.DataAnnotations;
using LocalScore.Api.Contracts.Authentication;

namespace LocalScore.Tests;

public sealed class RequestValidationTests
{
    [Fact]
    public void Empty_register_request_is_invalid()
    {
        var request = new RegisterRequest();
        var validationResults = Validate(request);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(request.Name)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(request.Email)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(request.Password)));
    }

    [Fact]
    public void Valid_register_request_passes_data_annotations()
    {
        var request = new RegisterRequest
        {
            Name = "João D'Ávila",
            Email = "joao@example.com",
            Password = "password1"
        };

        Assert.Empty(Validate(request));
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);

        return results;
    }
}
