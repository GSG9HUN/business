using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace DC_bot.Interface.Service.Persistence.MobileApps;

public interface IMobileAppSessionRepository
{
    Task CreateAsync(Guid sessionId, ulong discordUserId, string refreshTokenHash, DateTimeOffset expiresAtUtc, CancellationToken ct = default);
    Task<MobileAppSessionRecord?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);
    Task<bool> RotateRefreshTokenAsync(Guid sessionId, string currentRefreshTokenHash, string newRefreshTokenHash,
        DateTimeOffset expiresAtUtc, CancellationToken ct = default);
    Task RevokeAsync(Guid sessionId, CancellationToken ct = default);
}
