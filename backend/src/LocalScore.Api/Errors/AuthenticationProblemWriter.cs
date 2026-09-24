namespace LocalScore.Api.Errors;

internal static class AuthenticationProblemWriter
{
    public static async Task WriteAsync(
        HttpContext httpContext,
        int status,
        string title,
        string detail,
        string code)
    {
        var problem = ApiProblemDetailsFactory.Create(
            httpContext,
            status,
            title,
            detail,
            code);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, httpContext.RequestAborted);
    }
}
