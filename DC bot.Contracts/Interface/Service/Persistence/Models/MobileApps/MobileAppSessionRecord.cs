namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppSessionRecord(
    Guid SessionId,
    ulong DiscordUserId,
    string RefreshTokenHash,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc);