using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace DC_bot.Interface.Service.Persistence.MobileApps;

/// <summary>
/// Defines shared storage operations for mobile app session repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IMobileAppSessionRepository
{

    /// <summary>
    /// Persists a mobile refresh session with its user, hashed token, and expiry inside a transaction.
    /// </summary>
    /// <param name = "sessionId">The session id value used by this storage contract.</param>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "refreshTokenHash">The hashed refresh-token identity; plaintext token secrets are not stored here.</param>
    /// <param name = "expiresAtUtc">The UTC expiry used when creating or rotating the mobile refresh session.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task CreateAsync(
        Guid sessionId,
        ulong discordUserId,
        string refreshTokenHash,
        DateTimeOffset expiresAtUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Finds the persisted mobile session matching the hashed refresh token.
    /// </summary>
    /// <param name = "refreshTokenHash">The hashed refresh-token identity; plaintext token secrets are not stored here.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<MobileAppSessionRecord?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);

    /// <summary>
    /// Conditionally replaces a matching, unrevoked, unexpired refresh token so concurrent rotations cannot both succeed.
    /// </summary>
    /// <param name = "sessionId">The session id value used by this storage contract.</param>
    /// <param name = "currentRefreshTokenHash">The hashed refresh-token identity; plaintext token secrets are not stored here.</param>
    /// <param name = "newRefreshTokenHash">The hashed refresh-token identity; plaintext token secrets are not stored here.</param>
    /// <param name = "expiresAtUtc">The UTC expiry used when creating or rotating the mobile refresh session.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> RotateRefreshTokenAsync(
        Guid sessionId,
        string currentRefreshTokenHash,
        string newRefreshTokenHash,
        DateTimeOffset expiresAtUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Marks the identified mobile session revoked inside a transaction.
    /// </summary>
    /// <param name = "sessionId">The session id value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task RevokeAsync(Guid sessionId, CancellationToken ct = default);
}
