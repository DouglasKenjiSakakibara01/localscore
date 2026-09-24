using Microsoft.AspNetCore.Diagnostics;

namespace LocalScore.Api.Errors;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request {TraceId} was canceled by the client.", httpContext.TraceIdentifier);

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }

            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception while processing request {TraceId}.",
            httpContext.TraceIdentifier);

        var problem = ApiProblemDetailsFactory.Create(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred",
            "The server could not process the request.",
            "server.unexpected_error");

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
