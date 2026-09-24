using LocalScore.Infrastructure.Authentication;

namespace LocalScore.Tests;

public sealed class RefreshTokenFactoryTests
{
    [Fact]
    public void Generate_creates_distinct_opaque_tokens()
    {
        var first = RefreshTokenFactory.Generate();
        var second = RefreshTokenFactory.Generate();

        Assert.NotEqual(first, second);
        Assert.True(first.Length >= 64);
        Assert.DoesNotContain("=", first);
    }

    [Fact]
    public void Hash_is_deterministic_and_has_sha256_length()
    {
        var first = RefreshTokenFactory.Hash("token");
        var second = RefreshTokenFactory.Hash("token");

        Assert.Equal(32, first.Length);
        Assert.Equal(first, second);
        Assert.NotEqual(first, RefreshTokenFactory.Hash("another-token"));
    }
}
