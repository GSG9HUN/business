using DC_bot.Configuration;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Exceptions;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Validation;
using DC_bot.Wrapper.Lavalink;
using Microsoft.Extensions.Logging;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.PlaylistService;

/// <summary>
/// Loads and edits playlist tracks while enforcing configured playlist limits.
/// </summary>
/// <param name="playlistRepository">The playlist repository dependency used by this boundary.</param>
/// <param name="playlistTrackRepository">The playlist track repository dependency used by this boundary.</param>
/// <param name="trackSearchResolverService">The track search resolver service dependency used by this boundary.</param>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
/// <param name="trackLoader">The track loader supplied for this operation.</param>
/// <param name="options">The options supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class PlaylistTrackMutationService(
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    ITrackSearchResolverService trackSearchResolverService,
    ITrackSerializer trackSerializer,
    PlaylistTrackLoader trackLoader,
    PlaylistOptions options,
    ILogger<PlaylistService> logger)
{

    /// <summary>
    /// Loads source tracks into a saved playlist while enforcing names, ownership, and configured limits.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="playlistUrl">The playlist url supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<SavePlaylistResult> SavePlaylistAsync(
        ulong guildId,
        string playlistName,
        string playlistUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            logger.LogWarning("Invalid SavePlaylist request. GuildId: {GuildId}, PlaylistName: {PlaylistName}", guildId, playlistName);
            return SavePlaylistResult.InvalidPlaylistName;
        }

        playlistUrl = playlistUrl.Trim();

        if (string.IsNullOrWhiteSpace(playlistUrl))
        {
            logger.LogWarning("Invalid SavePlaylist URL. GuildId: {GuildId}, PlaylistName: {PlaylistName}, PlaylistUrl: {PlaylistUrl}", guildId, playlistName, playlistUrl);
            return SavePlaylistResult.UnknownError;
        }

        try
        {
            logger.LogInformation("Saving playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            var exists = await playlistRepository.ExistsAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (exists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
                return SavePlaylistResult.AlreadyExists;
            }

            if (await IsPlaylistLimitReachedAsync(guildId, cancellationToken))
            {
                var maxPlaylists = Math.Max(1, options.MaxPlaylistsPerGuild);
                logger.LogWarning("Playlist limit reached for guild {GuildId}. MaxPlaylistsPerGuild: {MaxPlaylistsPerGuild}", guildId, maxPlaylists);
                return SavePlaylistResult.PlaylistLimitReached;
            }

            var searchMode = trackSearchResolverService.ResolveSearchMode(playlistUrl);
            var loadResult = await trackLoader.LoadTracksAsync(playlistUrl, searchMode, "SavePlaylistAsync.LoadTracksAsync", cancellationToken);
            var tracks = ExtractTracks(loadResult);

            if (loadResult.IsFailed || tracks.Count == 0)
            {
                logger.LogWarning("No tracks found for playlist {PlaylistName} with URL {PlaylistUrl} for guild {GuildId}", playlistName, playlistUrl, guildId);
                return SavePlaylistResult.NoTracksFound;
            }

            var maxImportedTracks = GetMaxImportedTracks();
            var trackLimit = GetMaxTracksPerPlaylist();

            if (tracks.Count > maxImportedTracks || tracks.Count > trackLimit)
            {
                logger.LogWarning("Playlist {PlaylistName} for guild {GuildId} has {TrackCount} tracks, exceeding limits. MaxImportedTracks: {MaxImportedTracks}, MaxTracksPerPlaylist: {MaxTracksPerPlaylist}", playlistName, guildId, tracks.Count, maxImportedTracks, trackLimit);
                return SavePlaylistResult.TrackLimitExceeded;
            }

            var source = searchMode.ToString();
            var trackRecords = tracks.Select(track => CreateTrackRecord(source, trackSerializer.Serialize(new LavaLinkTrackWrapper(track)), playlistUrl))
                .ToList();
            var playlistCreated = false;
            try
            {
                var playlistId = await playlistRepository.CreatePlaylistAsync(guildId, playlistName, cancellationToken: cancellationToken);
                playlistCreated = true;
                await playlistTrackRepository.AddRangeAsync(playlistId, trackRecords, cancellationToken: cancellationToken);
            }
            catch
            {
                if (playlistCreated)
                {
                    await TryDeleteCreatedPlaylistAsync(guildId, playlistName, default);
                }

                throw;
            }

            logger.LogInformation("Saved playlist {PlaylistName} for guild {GuildId}. Track count: {TrackCount}", playlistName, guildId, trackRecords.Count);
            return SavePlaylistResult.Saved;
        }
        catch (UniqueConstraintConflictException ex)
        {
            logger.LogWarning(ex, "Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
            return SavePlaylistResult.AlreadyExists;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return SavePlaylistResult.UnknownError;
        }
    }

    /// <summary>
    /// Loads a source track and appends it when playlist naming and track limits permit it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="songUrl">The song url supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<AddSongResult> AddSongToPlaylistAsync(
        ulong guildId,
        string playlistName,
        string songUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            logger.LogWarning("Invalid AddSongToPlaylist request. GuildId: {GuildId}, PlaylistName: {PlaylistName}", guildId, playlistName);
            return AddSongResult.InvalidPlaylistName;
        }

        songUrl = songUrl.Trim();

        if (string.IsNullOrWhiteSpace(songUrl))
        {
            return AddSongResult.InvalidSongUrl;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return AddSongResult.PlaylistDoesNotExist;
            }

            var existingTracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id, cancellationToken: cancellationToken);

            if (existingTracks.Count >= GetMaxTracksPerPlaylist())
            {
                logger.LogWarning("Playlist {PlaylistName} in guild {GuildId} has reached the track limit {TrackLimit}", playlistName, guildId, options.MaxTracksPerPlaylist);
                return AddSongResult.TrackLimitReached;
            }

            var searchMode = trackSearchResolverService.ResolveSearchMode(songUrl);
            var loadedTrack = await trackLoader.LoadTracksAsync(songUrl, searchMode, "AddSongToPlaylistAsync.LoadTracksAsync", cancellationToken);

            if (loadedTrack.IsFailed)
            {
                return AddSongResult.InvalidSongUrl;
            }

            var tracks = ExtractTracks(loadedTrack);

            if (tracks.Count == 0)
            {
                return AddSongResult.NoTracksFound;
            }

            var track = tracks[0];
            var trackRecord = CreateTrackRecord(searchMode.ToString(), trackSerializer.Serialize(new LavaLinkTrackWrapper(track)), songUrl);
            await playlistTrackRepository.AddTrackAsync(playlist.Id, trackRecord, cancellationToken: cancellationToken);
            return AddSongResult.Added;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add song to the playlist {PlaylistName} for the guild {GuildId}", playlistName, guildId);
            return AddSongResult.UnknownError;
        }
    }

    /// <summary>
    /// Removes a numbered track from a guild-owned saved playlist.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="trackNumber">The track number supplied for this operation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    internal async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(
        ulong guildId,
        string playlistName,
        int trackNumber,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            return RemoveSongResult.InvalidPlaylistName;
        }

        if (trackNumber <= 0)
        {
            return RemoveSongResult.InvalidTrackNumber;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return RemoveSongResult.PlaylistDoesNotExist;
            }

            var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id, cancellationToken: cancellationToken);

            if (tracks.All(track => track.OrderNumber != trackNumber))
            {
                logger.LogInformation("Track number {TrackNumber} was not found in playlist {PlaylistName} for guild {GuildId}", trackNumber, playlistName, guildId);
                return RemoveSongResult.SongNotFound;
            }

            await playlistTrackRepository.RemoveTrackAsync(playlist.Id, trackNumber, cancellationToken: cancellationToken);
            logger.LogInformation("Removed track number {TrackNumber} from playlist {PlaylistName} for guild {GuildId}", trackNumber, playlistName, guildId);
            return RemoveSongResult.Removed;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove track number {TrackNumber} from playlist {PlaylistName} for guild {GuildId}", trackNumber, playlistName, guildId);
            return RemoveSongResult.UnknownError;
        }
    }

    /// <summary>
    /// Builds stored playlist-track values from a loaded track and its source metadata.
    /// </summary>
    /// <param name="source">The source supplied for this operation.</param>
    /// <param name="trackIdentifier">The track identifier supplied for this operation.</param>
    /// <param name="trackUri">The track uri supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static PlaylistTrackCreateRecord CreateTrackRecord(string source, string trackIdentifier, string trackUri)
    {
        return new PlaylistTrackCreateRecord(source, trackIdentifier, trackUri);
    }

    /// <summary>
    /// Checks the guild&apos;s playlist count against the configured limit.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    private async Task<bool> IsPlaylistLimitReachedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var maxPlaylists = Math.Max(1, options.MaxPlaylistsPerGuild);
        var playlists = await playlistRepository.GetByGuildAsync(guildId, cancellationToken: cancellationToken);
        return playlists.Count >= maxPlaylists;
    }

    /// <summary>
    /// Returns the configured saved-playlist track limit.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    private int GetMaxTracksPerPlaylist()
    {
        return Math.Max(1, options.MaxTracksPerPlaylist);
    }

    /// <summary>
    /// Returns the configured cap on imported source tracks.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    private int GetMaxImportedTracks()
    {
        return Math.Max(1, options.MaxImportedTracks);
    }

    /// <summary>
    /// Attempts to remove a newly created playlist when the surrounding save must be rolled back.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task TryDeleteCreatedPlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await playlistRepository.DeleteByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clean up playlist {PlaylistName} for guild {GuildId} after save failure", playlistName, guildId);
        }
    }

    /// <summary>
    /// Selects the single loaded track or playlist tracks from a Lavalink result.
    /// </summary>
    /// <param name="loadResult">The load result supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static List<LavalinkTrack> ExtractTracks(Lavalink4NET.Rest.Entities.Tracks.TrackLoadResult loadResult)
    {
        var tracks = loadResult.Tracks.ToList();

        if (tracks.Count == 0 && loadResult.Track is not null)
        {
            tracks.Add(loadResult.Track);
        }

        return tracks;
    }
}
