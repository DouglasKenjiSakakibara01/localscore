using LocalScore.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace LocalScore.Api.Errors;

internal static class ApiProblemDetailsFactory
{
    public static ObjectResult FromError(HttpContext httpContext, Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problem = error.Type == ErrorType.Validation &&
                                 error.ValidationErrors is not null
            ? new ValidationProblemDetails(error.ValidationErrors.ToDictionary())
            : new ProblemDetails();

        problem.Status = status;
        problem.Title = TitleFor(status);
        problem.Detail = error.Description;
        problem.Instance = httpContext.Request.Path;
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static ValidationProblemDetails FromModelState(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => ToCamelCase(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The value is invalid."
                        : error.ErrorMessage)
                    .ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path
        };

        problem.Extensions["code"] = "validation.failed";
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return problem;
    }

    public static ProblemDetails Create(
        HttpContext httpContext,
        int status,
        string title,
        string detail,
        string code)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Validation failed",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "An unexpected error occurred"
    };

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
}
