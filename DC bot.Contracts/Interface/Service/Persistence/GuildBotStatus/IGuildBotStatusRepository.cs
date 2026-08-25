using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;

namespace DC_bot.Interface.Service.Persistence.GuildBotStatus;

public interface IGuildBotStatusRepository
{
    Task UpsertConnectedVoiceAsync(
        ulong guildId,
        ulong voiceChannelId,
        string voiceChannelName,
        int voiceUserCount,
        CancellationToken ct = default);

    Task MarkDisconnectedVoiceAsync(
        ulong guildId,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<ulong, GuildBotStatusRecord>> GetByGuildIdsAsync(
        IReadOnlyCollection<ulong> guildIds,
        CancellationToken ct = default);
}