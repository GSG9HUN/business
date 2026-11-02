using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;

namespace DC_bot.Interface.Service.Persistence.GuildBotStatus;

/// <summary>
/// Defines shared storage operations for guild bot status repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IGuildBotStatusRepository
{

    /// <summary>
    /// Persists the guild&apos;s connected voice channel and user count, then publishes the matching voice event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "voiceChannelId">The voice channel id value used by this storage contract.</param>
    /// <param name = "voiceChannelName">The voice channel name value used by this storage contract.</param>
    /// <param name = "voiceUserCount">The voice user count value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task UpsertConnectedVoiceAsync(
        ulong guildId,
        ulong voiceChannelId,
        string voiceChannelName,
        int voiceUserCount,
        CancellationToken ct = default);

    /// <summary>
    /// Clears the guild&apos;s connected voice-channel state and publishes the disconnect event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task MarkDisconnectedVoiceAsync(ulong guildId, CancellationToken ct = default);

    /// <summary>
    /// Reads persisted voice status for the requested guild identifiers without tracking the returned entities.
    /// </summary>
    /// <param name = "guildIds">The guild identifiers whose persisted states are requested.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<IReadOnlyDictionary<ulong, GuildBotStatusRecord>> GetByGuildIdsAsync(
        IReadOnlyCollection<ulong> guildIds,
        CancellationToken ct = default);
}
