namespace DC_bot.Interface.Service.Persistence.Models.MobileApps;

public sealed record MobileAppUserUpsertRecord(
    ulong DiscordUserId,
    string Username,
    string? GlobalName,
    string? AvatarHash);