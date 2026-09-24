using System.Text.Json;
using LocalScore.Api.Errors;
using LocalScore.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalScore.Tests;

public sealed class ApiErrorHandlingTests
{
    [Fact]
    public void Expected_error_maps_to_problem_details()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-123"
        };
        context.Request.Path = "/sample";
        var error = Error.Conflict("sample.conflict", "A conflict occurred.");

        var response = ApiProblemDetailsFactory.FromError(context, error);
        var problem = Assert.IsType<ProblemDetails>(response.Value);

        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        Assert.Equal("sample.conflict", problem.Extensions["code"]);
        Assert.Equal("trace-123", problem.Extensions["traceId"]);
        Assert.Equal("A conflict occurred.", problem.Detail);
    }

    [Fact]
    public async Task Unexpected_exception_returns_generic_problem_and_keeps_details_out()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-456"
        };
        context.Request.Path = "/failure";
        context.Response.Body = new MemoryStream();

        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("sensitive database detail"),
            CancellationToken.None);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("server.unexpected_error", root.GetProperty("code").GetString());
        Assert.Equal("trace-456", root.GetProperty("traceId").GetString());
        Assert.DoesNotContain("sensitive", root.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Client_cancellation_does_not_return_500()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var context = new DefaultHttpContext
        {
            RequestAborted = cancellation.Token
        };
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            context,
            new OperationCanceledException(),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, context.Response.StatusCode);
    }
}
