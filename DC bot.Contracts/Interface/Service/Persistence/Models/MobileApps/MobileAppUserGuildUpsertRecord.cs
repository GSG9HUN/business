namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppUserGuildUpsertRecord(
    ulong GuildId,
    string Name,
    string? IconHash,
    ulong Permissions,
    bool IsOwner);