using System.IdentityModel.Tokens.Jwt;
using LocalScore.Application.Authentication;
using LocalScore.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace LocalScore.Tests;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void Generate_adds_approved_claims_and_expiration()
    {
        var now = new DateTimeOffset(2026, 9, 12, 15, 0, 0, TimeSpan.Zero);
        var options = Options.Create(new JwtOptions
        {
            Issuer = "LocalScore.Api",
            Audience = "LocalScore.Web",
            SigningKey = "a-test-signing-key-with-at-least-32-bytes",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });
        var generator = new JwtTokenGenerator(options, new FixedTimeProvider(now));
        var user = new AuthenticatedUser(
            Guid.Parse("9d2e4fe9-4cf4-4962-87e5-73a33403ef62"),
            "Douglas",
            "douglas@example.com");
        var sessionId = Guid.Parse("c78de7f7-5dc0-4502-b5d7-aa79ca19c342");

        var result = generator.Generate(user, sessionId);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Value);

        Assert.Equal(now.AddMinutes(15), result.ExpiresAt);
        Assert.Equal("LocalScore.Api", token.Issuer);
        Assert.Contains("LocalScore.Web", token.Audiences);
        Assert.Equal(user.Id.ToString(), token.Claims.Single(claim => claim.Type == "sub").Value);
        Assert.Equal(user.Email, token.Claims.Single(claim => claim.Type == "email").Value);
        Assert.Equal(user.Name, token.Claims.Single(claim => claim.Type == "name").Value);
        Assert.Equal(sessionId.ToString(), token.Claims.Single(claim => claim.Type == "sid").Value);
        Assert.Single(token.Claims, claim => claim.Type == "jti");
        Assert.Single(token.Claims, claim => claim.Type == "iat");
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
