using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

internal sealed class PlaylistQueryService(
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    PlaylistTrackDisplayMapper trackDisplayMapper,
    ILogger<PlaylistService> logger)
{
    internal async Task<PlaylistDto?> LoadPlaylistAsync(ulong guildId, string playlistName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);

        var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
        if (playlist is null)
        {
            logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
            return null;
        }

        var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id);

        return new PlaylistDto(
            playlist.Name,
            tracks.Select(PlaylistTrackDisplayMapper.MapToDto).ToList());
    }

    internal async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId)
    {
        try
        {
            var playlists = await playlistRepository.GetByGuildAsync(guildId);
            if (playlists.Count == 0)
            {
                logger.LogInformation("No playlists were found for guild {GuildId}", guildId);
                return new ListPlaylistsResult(ListPlaylistsStatus.NoPlaylists, []);
            }

            return new ListPlaylistsResult(
                ListPlaylistsStatus.Listed,
                playlists
                    .Select(playlist => new PlaylistSummaryDto(playlist.Name, playlist.TrackCount))
                    .ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list playlists for guild {GuildId}", guildId);
            return new ListPlaylistsResult(ListPlaylistsStatus.UnknownError, []);
        }
    }

    internal async Task<ViewPlaylistResult> ViewPlaylistAsync(ulong guildId, string playlistName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return new ViewPlaylistResult(ViewPlaylistStatus.PlaylistDoesNotExist, playlistName, []);
            }

            var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id);
            if (tracks.Count == 0)
            {
                logger.LogInformation("Playlist {PlaylistName} is empty for guild {GuildId}", playlistName, guildId);
                return new ViewPlaylistResult(ViewPlaylistStatus.EmptyPlaylist, playlist.Name, []);
            }

            var viewTracks = trackDisplayMapper.MapDisplayTracks(tracks, playlistName, guildId);
            if (viewTracks.Count == 0)
            {
                logger.LogWarning("Playlist {PlaylistName} for guild {GuildId} has no displayable tracks", playlistName,
                    guildId);
                return new ViewPlaylistResult(ViewPlaylistStatus.UnknownError, playlist.Name, []);
            }

            return new ViewPlaylistResult(ViewPlaylistStatus.Viewed, playlist.Name, viewTracks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to view playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return new ViewPlaylistResult(ViewPlaylistStatus.UnknownError, playlistName, []);
        }
    }
}
