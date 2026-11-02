using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.Models.Guilds;

namespace DC_bot.Service.Music.Lifecycle.PersistentVoice;

/// <summary>
/// Coordinates twenty-four-seven preferences and initial voice connection setup.
/// </summary>
/// <remarks>Shares one guild gate with periodic recovery and explicit leave; this service does not own the worker lifetime.</remarks>
/// <param name="guildDataRepository">Stores persistent guild preferences.</param>
/// <param name="persistentVoiceGate">Serializes preference changes with recovery and explicit leave.</param>
/// <param name="playerConnectionService">Joins or validates the existing voice player.</param>
public sealed class TwentyFourSevenService(
    IGuildDataRepository guildDataRepository,
    PersistentVoiceGate persistentVoiceGate,
    IPlayerConnectionService playerConnectionService)
{

    /// <summary>
    /// Reads the currently saved twenty-four-seven preferences.
    /// </summary>
    /// <param name="guildId">The Discord guild whose preferences are requested.</param>
    /// <param name="cancellationToken">Cancels the database query.</param>
    /// <returns>The persisted guild preferences.</returns>
    public Task<TwentyFourSevenSettingsRecord> GetAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        return guildDataRepository.GetTwentyFourSevenSettingsAsync(
            guildId,
            cancellationToken);
    }

    /// <summary>
    /// Changes persistent voice mode and establishes a player when enabling it.
    /// </summary>
    /// <param name="message">Provides guild context and validation response delivery.</param>
    /// <param name="enabled">Whether persistent voice mode should be enabled.</param>
    /// <param name="voiceChannel">The user's validated voice channel when enabling the mode.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream operations.</param>
    /// <returns>The saved preferences, or null when join validation already reported a failure.</returns>
    public async Task<TwentyFourSevenSettingsRecord?> SetAsync(
        IDiscordMessage message,
        bool enabled,
        IDiscordChannel? voiceChannel,
        CancellationToken cancellationToken = default)
    {
        var guildId = message.Channel.Guild.Id;
        var gate = persistentVoiceGate.Get(guildId);

        await gate.WaitAsync(cancellationToken);

        try
        {

            if (!enabled)
            {
                await guildDataRepository.SetTwentyFourSevenSettingsAsync(
                    guildId,
                    false,
                    null,
                    null,
                    cancellationToken);

                return await GetAsync(guildId, cancellationToken);
            }

            var joined = await playerConnectionService.TryJoinAndValidateAsync(
                message,
                voiceChannel,
                cancellationToken);

            if (!joined.isValid || joined.connection is null)
            {
                return null;
            }

            var channelId = joined.connection.VoiceChannelId;

            if (channelId == 0)
            {
                throw new InvalidOperationException(
                    "The validated player has no voice channel.");
            }

            await guildDataRepository.SetTwentyFourSevenSettingsAsync(
                guildId,
                true,
                channelId,
                message.Channel.Id,
                cancellationToken);

            return new TwentyFourSevenSettingsRecord(
                true,
                channelId,
                message.Channel.Id);
        }
        finally
        {
            gate.Release();
        }
    }
}
