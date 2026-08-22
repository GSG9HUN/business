namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppUserGuildRecord(
    ulong GuildId,
    string Name,
    string? IconHash,
    ulong Permissions,
    bool IsOwner,
    DateTimeOffset LastSeenAtUtc);