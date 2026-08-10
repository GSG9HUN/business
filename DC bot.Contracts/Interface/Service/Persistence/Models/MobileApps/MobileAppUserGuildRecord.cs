namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppUserGuildRecord(
    ulong GuildId,
    ulong Permissions,
    bool IsOwner,
    DateTimeOffset LastSeenAtUtc);