namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppUserGuildUpsertRecord(
    ulong GuildId,
    ulong Permissions,
    bool IsOwner);