using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

internal sealed class PlaylistTrackMutationService(
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    ITrackSearchResolverService trackSearchResolverService,
    ITrackSerializer trackSerializer,
    PlaylistTrackLoader trackLoader,
    ILogger<PlaylistService> logger)
{
    internal async Task<SavePlaylistResult> SavePlaylistAsync(ulong guildId, string playlistName, string playlistUrl)
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
            var loadResult = await trackLoader.LoadTracksAsync(
                playlistUrl,
                searchMode,
                "SavePlaylistAsync.LoadTracksAsync");
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
                .Select(track => CreateTrackRecord(source, trackSerializer.Serialize(new LavaLinkTrackWrapper(track)), playlistUrl))
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

    internal async Task<AddSongResult> AddSongToPlaylistAsync(ulong guildId, string playlistName, string songUrl)
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
            var loadedTrack = await trackLoader.LoadTracksAsync(
                songUrl,
                searchMode,
                "AddSongToPlaylistAsync.LoadTracksAsync");

            if (loadedTrack.IsFailed)
            {
                return AddSongResult.InvalidSongUrl;
            }

            if (loadedTrack.Count == 0 || loadedTrack.Tracks.Length == 0)
            {
                return AddSongResult.NoTracksFound;
            }

            var track = loadedTrack.Tracks[0];
            var trackRecord = CreateTrackRecord(
                searchMode.ToString(),
                trackSerializer.Serialize(new LavaLinkTrackWrapper(track)),
                songUrl);

            await playlistTrackRepository.AddTrackAsync(playlist.Id, trackRecord);

            return AddSongResult.Added;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add song to the playlist {PlaylistName} for the guild {GuildId}", playlistName, guildId);
            return AddSongResult.UnknownError;
        }
    }

    internal async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(ulong guildId, string playlistName, int trackNumber)
    {
        playlistName = playlistName.Trim();

        if (!PlaylistNameValidator.IsValid(playlistName))
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

    private static PlaylistTrackCreateRecord CreateTrackRecord(
        string source,
        string trackIdentifier,
        string trackUri)
    {
        return new PlaylistTrackCreateRecord(source, trackIdentifier, trackUri);
    }
}
