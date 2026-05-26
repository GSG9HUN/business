using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.MusicServices;

public class RepeatService(
    IPlaybackStateRepository playbackStateRepository,
    IRepeatListRepository repeatListRepository,
    ILogger<RepeatService>? logger = null) : IRepeatService
{
    private readonly ILogger<RepeatService> _logger = logger ?? NullLogger<RepeatService>.Instance;

    public async Task InitAsync(ulong guildId)
    {
        await playbackStateRepository.GetOrCreateAsync(guildId);
        _logger.LogDebug("Repeat state initialized for guild {GuildId}.", guildId);
    }

    public async Task<bool> IsRepeatingAsync(ulong guildId)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        _logger.LogDebug("Single-track repeat state queried for guild {GuildId}. Enabled: {IsRepeating}",
            guildId,
            state.IsRepeating);
        return state.IsRepeating;
    }

    public async Task SetRepeatingAsync(ulong guildId, bool value)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        await playbackStateRepository.SetRepeatStateAsync(guildId, value, state.IsRepeatingList);
        _logger.LogInformation(
            "Single-track repeat state updated for guild {GuildId}. Previous: {PreviousValue}, New: {NewValue}",
            guildId,
            state.IsRepeating,
            value);
    }

    public async Task<bool> IsRepeatingListAsync(ulong guildId)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        _logger.LogDebug("Repeat-list state queried for guild {GuildId}. Enabled: {IsRepeatingList}",
            guildId,
            state.IsRepeatingList);
        return state.IsRepeatingList;
    }

    public async Task SetRepeatingListAsync(ulong guildId, bool value)
    {
        var state = await playbackStateRepository.GetOrCreateAsync(guildId);
        await playbackStateRepository.SetRepeatStateAsync(guildId, state.IsRepeating, value);
        _logger.LogInformation(
            "Repeat-list state updated for guild {GuildId}. Previous: {PreviousValue}, New: {NewValue}",
            guildId,
            state.IsRepeatingList,
            value);

        if (!value)
        {
            await repeatListRepository.ClearAsync(guildId);
            _logger.LogInformation("Repeat-list snapshot cleared for guild {GuildId}.", guildId);
        }
    }

    public async Task SaveRepeatListSnapshotAsync(
        ulong guildId,
        ILavaLinkTrack? currentTrack,
        IReadOnlyCollection<ILavaLinkTrack> queuedTracks)
    {
        ArgumentNullException.ThrowIfNull(queuedTracks);

        var trackIdentifiers = new List<string>(queuedTracks.Count + (currentTrack is null ? 0 : 1));
        if (currentTrack is not null)
        {
            trackIdentifiers.Add(currentTrack.ToString());
        }

        trackIdentifiers.AddRange(queuedTracks.Select(track => track.ToString()));

        await repeatListRepository.ReplaceAsync(guildId, trackIdentifiers);
        _logger.LogInformation(
            "Repeat-list snapshot saved for guild {GuildId}. Track count: {TrackCount}, Includes current track: {IncludesCurrentTrack}",
            guildId,
            trackIdentifiers.Count,
            currentTrack is not null);
    }

}
