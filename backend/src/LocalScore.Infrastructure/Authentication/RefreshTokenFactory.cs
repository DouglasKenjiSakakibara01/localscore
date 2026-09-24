using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LocalScore.Infrastructure.Authentication;

public static class RefreshTokenFactory
{
    public static string Generate() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

    public static byte[] Hash(string token) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(token));
}
