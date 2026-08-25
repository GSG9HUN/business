using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Services.Auth;

public sealed class AppTokenService(IConfiguration configuration)
{
    public const int AccessTokenExpiresInSeconds = 900;
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    public string CreateAccessToken(Guid sessionId, ulong discordUserId)
    {
        var key = configuration["AppAuth:SigningKey"]
                  ?? throw new InvalidOperationException("Missing AppAuth:SigningKey.");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["AppAuth:Issuer"],
            audience: configuration["AppAuth:Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, discordUserId.ToString()),
                new Claim("sid", sessionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: DateTime.UtcNow.AddSeconds(AccessTokenExpiresInSeconds),
            signingCredentials: credentials);

        return TokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}