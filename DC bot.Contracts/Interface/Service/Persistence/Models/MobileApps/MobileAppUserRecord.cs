namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppUserRecord(
    ulong DiscordUserId,
    string Username,
    string? GlobalName,
    string? AvatarHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastLoginAtUtc);