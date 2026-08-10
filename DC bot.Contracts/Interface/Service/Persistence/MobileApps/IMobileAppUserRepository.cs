using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace DC_bot.Interface.Service.Persistence.MobileApps;

public interface IMobileAppUserRepository
{
    Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken ct = default);
    Task SyncUserGuildsAsync(ulong discordUserId, IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds, CancellationToken ct = default);
    Task<MobileAppUserRecord?> GetUserAsync(ulong discordUserId, CancellationToken ct = default);
    Task<IReadOnlyList<MobileAppUserGuildRecord>> GetGuildsForUserAsync(ulong discordUserId, CancellationToken ct = default);
    Task<bool> HasGuildAccessAsync(ulong discordUserId, ulong guildId, CancellationToken ct = default);
}