using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Playlists;
using DC_bot.Validation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

/// <summary>
/// Reads saved playlists and restores displayable tracks from persisted records.
/// </summary>
/// <param name="playlistRepository">The playlist repository dependency used by this boundary.</param>
/// <param name="playlistTrackRepository">The playlist track repository dependency used by this boundary.</param>
/// <param name="trackDisplayMapper">The track display mapper supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class PlaylistQueryService(
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    PlaylistTrackDisplayMapper trackDisplayMapper,
    ILogger<PlaylistService> logger)
{

    /// <summary>
    /// Restores a saved playlist&apos;s tracks into a playback-ready load result.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<LoadPlaylistResult> LoadPlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
            {
                return new LoadPlaylistResult(LoadPlaylistStatus.InvalidPlaylistName, null);
            }

            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return new LoadPlaylistResult(LoadPlaylistStatus.NotFound, null);
            }

            var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id, cancellationToken: cancellationToken);

            if (tracks.Count == 0)
            {
                logger.LogInformation("Playlist {PlaylistName} is empty for guild {GuildId}", playlistName, guildId);
                return new LoadPlaylistResult(LoadPlaylistStatus.EmptyPlaylist, null);
            }

            return new LoadPlaylistResult(LoadPlaylistStatus.Loaded, new PlaylistDto(playlist.Name, tracks.Select(PlaylistTrackDisplayMapper.MapToDto)
                .ToList()));
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return new LoadPlaylistResult(LoadPlaylistStatus.UnknownError, null);
        }
    }

    /// <summary>
    /// Reads the guild&apos;s saved playlists and track counts for a list response.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var playlists = await playlistRepository.GetByGuildAsync(guildId, cancellationToken: cancellationToken);

            if (playlists.Count == 0)
            {
                logger.LogInformation("No playlists were found for guild {GuildId}", guildId);
                return new ListPlaylistsResult(ListPlaylistsStatus.NoPlaylists, []);
            }

            return new ListPlaylistsResult(ListPlaylistsStatus.Listed, playlists.Select(playlist => new PlaylistSummaryDto(playlist.Name, playlist.TrackCount))
                .ToList());
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list playlists for guild {GuildId}", guildId);
            return new ListPlaylistsResult(ListPlaylistsStatus.UnknownError, []);
        }
    }

    /// <summary>
    /// Reads and restores display metadata for a named guild playlist.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<ViewPlaylistResult> ViewPlaylistAsync(
        ulong guildId,
        string playlistName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            return new ViewPlaylistResult(ViewPlaylistStatus.InvalidPlaylistName, playlistName, []);
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken: cancellationToken);

            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return new ViewPlaylistResult(ViewPlaylistStatus.PlaylistDoesNotExist, playlistName, []);
            }

            var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id, cancellationToken: cancellationToken);

            if (tracks.Count == 0)
            {
                logger.LogInformation("Playlist {PlaylistName} is empty for guild {GuildId}", playlistName, guildId);
                return new ViewPlaylistResult(ViewPlaylistStatus.EmptyPlaylist, playlist.Name, []);
            }

            var viewTracks = trackDisplayMapper.MapDisplayTracks(tracks, playlistName, guildId);

            if (viewTracks.Count == 0)
            {
                logger.LogWarning("Playlist {PlaylistName} for guild {GuildId} has no displayable tracks", playlistName, guildId);
                return new ViewPlaylistResult(ViewPlaylistStatus.UnknownError, playlist.Name, []);
            }

            return new ViewPlaylistResult(ViewPlaylistStatus.Viewed, playlist.Name, viewTracks);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to view playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return new ViewPlaylistResult(ViewPlaylistStatus.UnknownError, playlistName, []);
        }
    }
}
