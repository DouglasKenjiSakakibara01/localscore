using LocalScore.Application.Common;

namespace LocalScore.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_contains_value_and_no_error()
    {
        var result = Result<string>.Success("ok");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("ok", result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_contains_error_and_value_is_inaccessible()
    {
        var error = Error.NotFound("sample.not_found", "Not found.");
        var result = Result<string>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Result_rejects_an_inconsistent_state()
    {
        Assert.Throws<ArgumentException>(() => InvalidResult.Create());
    }

    private sealed class InvalidResult : Result
    {
        private InvalidResult() : base(true, Error.NotFound("invalid", "Invalid"))
        {
        }

        public static InvalidResult Create() => new();
    }
}
