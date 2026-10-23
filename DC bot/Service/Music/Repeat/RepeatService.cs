using DC_bot.BotControl;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Playback;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.Repeat;

/// <summary>
/// Persists repeat modes and maintains the legacy serialized repeat-list snapshot.
/// </summary>
/// <param name="playbackStateRepository">The playback state repository dependency used by this boundary.</param>
/// <param name="repeatListRepository">The repeat list repository supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
public class RepeatService(
    IPlaybackStateRepository playbackStateRepository,
    IRepeatListRepository repeatListRepository,
    ITrackSerializer trackSerializer,
    ILogger<RepeatService>? logger = null) : IRepeatService
{
    private readonly ILogger<RepeatService> _logger = logger ?? NullLogger<RepeatService>.Instance;
    private readonly ITrackSerializer _trackSerializer = trackSerializer ?? throw new ArgumentNullException(nameof(trackSerializer));

    /// <summary>
    /// Ensures persisted playback and repeat state exists for the guild.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task InitAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken: cancellationToken);
        _logger.LogDebug("Repeat state initialized for guild {GuildId}.", guildId);
    }

    /// <summary>
    /// Reads the guild&apos;s persisted single-track repeat mode.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> IsRepeatingAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken: cancellationToken);
        _logger.LogDebug("Single-track repeat state queried for guild {GuildId}. Enabled: {IsRepeating}", guildId, state.IsRepeating);
        return state.IsRepeating;
    }

    /// <summary>
    /// Persists single-track repeat while retaining list-repeat state and publishing the mode change.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="value">The value supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SetRepeatingAsync(ulong guildId, bool value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken: cancellationToken);
        await playbackStateRepository.SetRepeatStateAsync(guildId, value, state.IsRepeatingList, MobileRealtimeEventNames.RepeatModeChanged, cancellationToken: cancellationToken);
        _logger.LogInformation("Single-track repeat state updated for guild {GuildId}. Previous: {PreviousValue}, New: {NewValue}", guildId, state.IsRepeating, value);
    }

    /// <summary>
    /// Reads the guild&apos;s persisted list-repeat mode.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> IsRepeatingListAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken: cancellationToken);
        _logger.LogDebug("Repeat-list state queried for guild {GuildId}. Enabled: {IsRepeatingList}", guildId, state.IsRepeatingList);
        return state.IsRepeatingList;
    }

    /// <summary>
    /// Persists list-repeat mode and clears the legacy repeat snapshot when disabled.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="value">The value supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SetRepeatingListAsync(ulong guildId, bool value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken: cancellationToken);
        await playbackStateRepository.SetRepeatStateAsync(guildId, state.IsRepeating, value, MobileRealtimeEventNames.RepeatModeChanged, cancellationToken: cancellationToken);
        _logger.LogInformation("Repeat-list state updated for guild {GuildId}. Previous: {PreviousValue}, New: {NewValue}", guildId, state.IsRepeatingList, value);

        if (!value)
        {
            await repeatListRepository.ClearAsync(guildId, cancellationToken: cancellationToken);
            _logger.LogInformation("Repeat-list snapshot cleared for guild {GuildId}.", guildId);
        }
    }

    /// <summary>
    /// Serializes the supplied current and waiting tracks into the legacy repeat-list snapshot.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="currentTrack">The lava link track? dependency used by this boundary.</param>
    /// <param name="queuedTracks">The queued tracks supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SaveRepeatListSnapshotAsync(
        ulong guildId,
        ILavaLinkTrack? currentTrack,
        IReadOnlyCollection<ILavaLinkTrack> queuedTracks,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(queuedTracks);
        var trackIdentifiers = new List<string>(queuedTracks.Count + (currentTrack is null ? 0 : 1));

        if (currentTrack is not null)
        {
            trackIdentifiers.Add(_trackSerializer.Serialize(currentTrack));
        }

        trackIdentifiers.AddRange(queuedTracks.Select(_trackSerializer.Serialize));
        await repeatListRepository.ReplaceAsync(guildId, trackIdentifiers, cancellationToken: cancellationToken);
        _logger.LogInformation("Repeat-list snapshot saved for guild {GuildId}. Track count: {TrackCount}, Includes current track: {IncludesCurrentTrack}", guildId, trackIdentifiers.Count, currentTrack is not null);
    }
}
