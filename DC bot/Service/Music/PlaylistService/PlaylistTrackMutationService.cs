using DC_bot.Configuration;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.PlaylistService;

internal sealed class PlaylistTrackMutationService(
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    ITrackSearchResolverService trackSearchResolverService,
    ITrackSerializer trackSerializer,
    PlaylistTrackLoader trackLoader,
    PlaylistOptions options,
    ILogger<PlaylistService> logger)
{
    internal async Task<SavePlaylistResult> SavePlaylistAsync(ulong guildId, string playlistName, string playlistUrl)
    {
        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            logger.LogWarning(
                "Invalid SavePlaylist request. GuildId: {GuildId}, PlaylistName: {PlaylistName}",
                guildId,
                playlistName);
            return SavePlaylistResult.InvalidPlaylistName;
        }

        playlistUrl = playlistUrl.Trim();

        if (string.IsNullOrWhiteSpace(playlistUrl))
        {
            logger.LogWarning(
                "Invalid SavePlaylist URL. GuildId: {GuildId}, PlaylistName: {PlaylistName}, PlaylistUrl: {PlaylistUrl}",
                guildId,
                playlistName,
                playlistUrl);
            return SavePlaylistResult.UnknownError;
        }

        try
        {
            logger.LogInformation("Saving playlist {PlaylistName} for guild {GuildId}", playlistName, guildId);

            var exists = await playlistRepository.ExistsAsync(guildId, playlistName);
            if (exists)
            {
                logger.LogWarning("Playlist {PlaylistName} already exists for guild {GuildId}", playlistName, guildId);
                return SavePlaylistResult.AlreadyExists;
            }

            if (await IsPlaylistLimitReachedAsync(guildId))
            {
                logger.LogWarning(
                    "Playlist limit reached for guild {GuildId}. MaxPlaylistsPerGuild: {MaxPlaylistsPerGuild}",
                    guildId,
                    options.MaxPlaylistsPerGuild);
                return SavePlaylistResult.PlaylistLimitReached;
            }

            var searchMode = trackSearchResolverService.ResolveSearchMode(playlistUrl);
            var loadResult = await trackLoader.LoadTracksAsync(
                playlistUrl,
                searchMode,
                "SavePlaylistAsync.LoadTracksAsync");
            var tracks = ExtractTracks(loadResult);

            if (loadResult.IsFailed || tracks.Count == 0)
            {
                logger.LogWarning(
                    "No tracks found for playlist {PlaylistName} with URL {PlaylistUrl} for guild {GuildId}",
                    playlistName,
                    playlistUrl,
                    guildId);
                return SavePlaylistResult.NoTracksFound;
            }

            var trackLimit = GetMaxTracksPerPlaylist();
            if (tracks.Count > GetMaxImportedTracks() || tracks.Count > trackLimit)
            {
                logger.LogWarning(
                    "Playlist {PlaylistName} for guild {GuildId} has {TrackCount} tracks, exceeding the limit {TrackLimit}",
                    playlistName,
                    guildId,
                    tracks.Count,
                    trackLimit);
                return SavePlaylistResult.TrackLimitExceeded;
            }

            var source = searchMode.ToString();
            var trackRecords = tracks
                .Select(track => CreateTrackRecord(source, trackSerializer.Serialize(new LavaLinkTrackWrapper(track)), playlistUrl))
                .ToList();

            var playlistCreated = false;
            try
            {
                var playlistId = await playlistRepository.CreatePlaylistAsync(guildId, playlistName);
                playlistCreated = true;
                await playlistTrackRepository.AddRangeAsync(playlistId, trackRecords);
            }
            catch
            {
                if (playlistCreated)
                {
                    await TryDeleteCreatedPlaylistAsync(guildId, playlistName);
                }

                throw;
            }

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
        if (!PlaylistNameValidator.TryNormalize(playlistName, out playlistName))
        {
            logger.LogWarning(
                "Invalid AddSongToPlaylist request. GuildId: {GuildId}, PlaylistName: {PlaylistName}",
                guildId,
                playlistName);
            return AddSongResult.InvalidPlaylistName;
        }

        songUrl = songUrl.Trim();

        if (string.IsNullOrWhiteSpace(songUrl))
        {
            return AddSongResult.InvalidSongUrl;
        }

        try
        {
            var playlist = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName);
            if (playlist is null)
            {
                logger.LogInformation("Playlist {PlaylistName} was not found for guild {GuildId}", playlistName, guildId);
                return AddSongResult.PlaylistDoesNotExist;
            }

            var existingTracks = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlist.Id);
            if (existingTracks.Count >= GetMaxTracksPerPlaylist())
            {
                logger.LogWarning(
                    "Playlist {PlaylistName} in guild {GuildId} has reached the track limit {TrackLimit}",
                    playlistName,
                    guildId,
                    options.MaxTracksPerPlaylist);
                return AddSongResult.TrackLimitReached;
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

            var tracks = ExtractTracks(loadedTrack);
            if (tracks.Count == 0)
            {
                return AddSongResult.NoTracksFound;
            }

            var track = tracks[0];
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

    private async Task<bool> IsPlaylistLimitReachedAsync(ulong guildId)
    {
        var maxPlaylists = Math.Max(1, options.MaxPlaylistsPerGuild);
        var playlists = await playlistRepository.GetByGuildAsync(guildId);
        return playlists.Count >= maxPlaylists;
    }

    private int GetMaxTracksPerPlaylist()
    {
        return Math.Max(1, options.MaxTracksPerPlaylist);
    }

    private int GetMaxImportedTracks()
    {
        return Math.Max(1, options.MaxImportedTracks);
    }

    private async Task TryDeleteCreatedPlaylistAsync(ulong guildId, string playlistName)
    {
        try
        {
            await playlistRepository.DeleteByGuildAndNameAsync(guildId, playlistName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to clean up playlist {PlaylistName} for guild {GuildId} after save failure",
                playlistName,
                guildId);
        }
    }

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
