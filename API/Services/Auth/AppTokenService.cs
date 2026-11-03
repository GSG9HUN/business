using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services.Auth;

/// <summary>
/// Creates signed mobile access tokens and cryptographically random refresh tokens and their hashes.
/// </summary>
/// <param name="options">The injected typed configuration for this component.</param>
public sealed class AppTokenService(IOptions<AppAuthOptions> options)
{

    /// <summary>
    /// The access token lifetime in seconds.
    /// </summary>
    public const int AccessTokenExpiresInSeconds = 900;
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    /// <summary>
    /// Signs a short-lived JWT containing the Discord subject, session identifier, and unique token identifier.
    /// </summary>
    /// <param name="sessionId">The session id value for this operation.</param>
    /// <param name="discordUserId">The discord user id value for this operation.</param>
    /// <returns>The computed create access token result.</returns>
    public string CreateAccessToken(Guid sessionId, ulong discordUserId)
    {
        var key = options.Value.SigningKey;
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: options.Value.Issuer, audience: options.Value.Audience, claims: [new Claim(JwtRegisteredClaimNames.Sub, discordUserId.ToString()), new Claim("sid", sessionId.ToString()), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid()
            .ToString())], expires: DateTime.UtcNow.AddSeconds(AccessTokenExpiresInSeconds), signingCredentials: credentials);
        return TokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Creates a Base64-encoded refresh token from 64 cryptographically random bytes.
    /// </summary>
    /// <returns>The computed create refresh token result.</returns>
    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    /// <summary>
    /// Hashes a refresh token with SHA-256 for persistence without storing the raw token.
    /// </summary>
    /// <param name="refreshToken">The refresh token value for this operation.</param>
    /// <returns>The computed hash refresh token result.</returns>
    public string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}
