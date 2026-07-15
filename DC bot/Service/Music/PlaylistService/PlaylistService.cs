using DC_bot.Exceptions.Music;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Logging;
using DC_bot.Wrapper;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

public class PlaylistService(
    IAudioService audioService,
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    ITrackSearchResolverService trackSearchResolverService,
    ITrackSerializer trackSerializer,
    ILogger<PlaylistService> logger) : IPlaylistService
{
    public async Task<SavePlaylistResult> SavePlaylistAsync(ulong guildId, string playlistName, string playlistUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistUrl);

        try
        {
            logger.LogInformation("Saving playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);

            var exists = await playlistRepository.ExistsAsync(guildId, playlistName);
            if (exists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
                return SavePlaylistResult.AlreadyExists;
            }

            var searchMode = trackSearchResolverService.ResolveSearchMode(playlistUrl);
            var loadResult = await LoadTracksAsync(playlistUrl, searchMode);
            var tracks = loadResult.Tracks.ToList();

            if (tracks.Count == 0 && loadResult.Track is not null)
            {
                tracks.Add(loadResult.Track);
            }

            if (loadResult.IsFailed || tracks.Count == 0)
            {
                logger.LogWarning(
                    "No tracks found for playlist {PlaylistName} with URL {PlaylistUrl} for guild {GuildId}",
                    playlistName,
                    playlistUrl,
                    guildId);
                return SavePlaylistResult.NoTracksFound;
            }

            var playlistId = await playlistRepository.CreatePlaylistAsync(guildId, playlistName);
            var source = searchMode.ToString();
            var trackRecords = tracks
                .Select(track => new PlaylistTrackCreateRecord(
                    source,
                    trackSerializer.Serialize(new LavaLinkTrackWrapper(track)),
                    playlistUrl))
                .ToList();

            await playlistTrackRepository.AddRangeAsync(playlistId, trackRecords);

            logger.LogInformation("Saved playlist {PlaylistName} for guild {GuildId}. Track count: {TrackCount}",
                playlistName,
                guildId,
                trackRecords.Count);

            return SavePlaylistResult.Saved;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return SavePlaylistResult.UnknownError;
        }
    }

    public async Task<PlaylistDto?> LoadPlaylistAsync(ulong guildId, string playlistName)
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
            tracks.Select(MapToDto).ToList());
    }

    public async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId)
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

    public async Task<ViewPlaylistResult> ViewPlaylistAsync(ulong guildId, string playlistName)
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

            var viewTracks = new List<PlaylistViewTrackDto>(tracks.Count);
            foreach (var track in tracks)
            {
                try
                {
                    var lavaLinkTrack = trackSerializer.Deserialize(track.TrackIdentifier);
                    viewTracks.Add(new PlaylistViewTrackDto(
                        track.OrderNumber,
                        lavaLinkTrack.Title,
                        lavaLinkTrack.Author,
                        lavaLinkTrack.Duration,
                        track.TrackUri));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Skipping unparsable track in playlist {PlaylistName} for guild {GuildId}. PlaylistTrackId: {PlaylistTrackId}",
                        playlistName,
                        guildId,
                        track.Id);
                }
            }

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

    public async Task<DeletePlaylistResult> DeletePlaylistAsync(ulong guildId, string playlistName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return DeletePlaylistResult.DoesNotExist;
            }

            var deleted = await playlistRepository.DeleteByGuildAndNameAsync(guildId, playlistName);

            return deleted ? DeletePlaylistResult.Deleted : DeletePlaylistResult.UnknownError;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return DeletePlaylistResult.UnknownError;
        }
    }

    public async Task<AddSongResult> AddSongToPlaylistAsync(ulong guildId, string playlistName, string songUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistName);

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return AddSongResult.PlaylistDoesNotExist;
            }

            var searchMode = trackSearchResolverService.ResolveSearchMode(songUrl);

            var loadedTrack = await LoadTracksAsync(songUrl, searchMode);

            if (loadedTrack.IsFailed)
            {
                return AddSongResult.InvalidSongUrl;
            }

            if (loadedTrack.Count == 0 || loadedTrack.Tracks.Length == 0)
            {
                return AddSongResult.NoTracksFound;
            }

            var track = loadedTrack.Tracks[0];

            var trackRecord = new PlaylistTrackCreateRecord(searchMode.ToString(), trackSerializer.Serialize(new LavaLinkTrackWrapper(track)), songUrl);

            await playlistTrackRepository.AddTrackAsync(playlist.Id, trackRecord);

            return AddSongResult.Added;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add song to the playlist {PlaylistName} for the guild {GuildId}", playlistName, guildId);
            return AddSongResult.UnknownError;
        }
    }

    public async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(ulong guildId, string playlistName, int trackNumber)
    {
        playlistName = playlistName.Trim();

        if (!IsValidPlaylistName(playlistName))
        {
            return RemoveSongResult.InvalidPlaylistName;
        }

        if (trackNumber <= 0)
        {
            return RemoveSongResult.InvalidTrackNumber;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return RemoveSongResult.PlaylistDoesNotExist;
            }

            var tracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id);
            if (tracks.All(track => track.OrderNumber != trackNumber))
            {
                logger.LogInformation(
                    "Track number {TrackNumber} was not found in playlist {PlaylistName} for guild {GuildId}",
                    trackNumber,
                    playlistName,
                    guildId);
                return RemoveSongResult.SongNotFound;
            }

            await playlistTrackRepository.RemoveTrackAsync(playlist.Id, trackNumber);
            logger.LogInformation(
                "Removed track number {TrackNumber} from playlist {PlaylistName} for guild {GuildId}",
                trackNumber,
                playlistName,
                guildId);

            return RemoveSongResult.Removed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to remove track number {TrackNumber} from playlist {PlaylistName} for guild {GuildId}",
                trackNumber,
                playlistName,
                guildId);
            return RemoveSongResult.UnknownError;
        }
    }

    public async Task<RenamePlaylistResult> RenamePlaylistAsync(ulong guildId, string currentName, string newName)
    {
        currentName = currentName.Trim();
        newName = newName.Trim();

        if (!IsValidPlaylistName(currentName) || !IsValidPlaylistName(newName))
        {
            return RenamePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, currentName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", currentName, guildId);
                return RenamePlaylistResult.PlaylistDoesNotExist;
            }

            var newNameExists = await playlistRepository.ExistsAsync(guildId, newName);
            if (newNameExists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", newName, guildId);
                return RenamePlaylistResult.PlaylistAlreadyExists;
            }

            var renamed = await playlistRepository.RenameAsync(guildId, currentName, newName);
            if (!renamed)
            {
                return RenamePlaylistResult.UnknownError;
            }

            logger.LogInformation("Renamed playlist {CurrentName} to {NewName} for guild {GuildId}", currentName,
                newName, guildId);
            return RenamePlaylistResult.Renamed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rename playlist {CurrentName} to {NewName} for guild {GuildId}", currentName,
                newName, guildId);
            return RenamePlaylistResult.UnknownError;
        }
    }

    public async Task<CreatePlaylistResult> CreatePlaylistAsync(ulong guildId, string playlistName)
    {
        playlistName = playlistName.Trim();

        if (!IsValidPlaylistName(playlistName))
        {
            return CreatePlaylistResult.InvalidPlaylistName;
        }

        try
        {
            var exists = await playlistRepository.ExistsAsync(guildId, playlistName);

            if (exists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
                return CreatePlaylistResult.PlaylistAlreadyExists;
            }

            await playlistRepository.CreatePlaylistAsync(guildId, playlistName);
            logger.LogInformation("Created new playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.Created;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);
            return CreatePlaylistResult.UnknownError;
        }
    }

    private async Task<TrackLoadResult> LoadTracksAsync(string playlistUrl, TrackSearchMode searchMode)
    {
        try
        {
            return await audioService.Tracks.LoadTracksAsync(playlistUrl, searchMode).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "SavePlaylistAsync.LoadTracksAsync");
            throw new TrackLoadException(playlistUrl, "Failed to load playlist tracks", ex);
        }
    }

    private static PlaylistTrackDto MapToDto(PlaylistTrackRecord track)
    {
        return new PlaylistTrackDto(
            track.OrderNumber,
            track.Source,
            track.TrackIdentifier,
            track.TrackUri);
    }
    
    private static bool IsValidPlaylistName(string playlistName)
    {
        return playlistName.Length is >= 1 and <= 64
               && !playlistName.Contains('\n')
               && !playlistName.Contains('\r');
    }
}
