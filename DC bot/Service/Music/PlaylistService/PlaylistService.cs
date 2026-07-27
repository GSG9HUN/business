using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

public class PlaylistService : IPlaylistService
{
    private readonly PlaylistMutationService _mutationService;
    private readonly PlaylistQueryService _queryService;
    private readonly PlaylistTrackMutationService _trackMutationService;

    public PlaylistService(
        IAudioService audioService,
        IPlaylistRepository playlistRepository,
        IPlaylistTrackRepository playlistTrackRepository,
        ITrackSearchResolverService trackSearchResolverService,
        ITrackSerializer trackSerializer,
        ILogger<PlaylistService> logger)
    {
        var trackLoader = new PlaylistTrackLoader(audioService, logger);
        var trackDisplayMapper = new PlaylistTrackDisplayMapper(trackSerializer, logger);

        _queryService = new PlaylistQueryService(
            playlistRepository,
            playlistTrackRepository,
            trackDisplayMapper,
            logger);
        _mutationService = new PlaylistMutationService(playlistRepository, logger);
        _trackMutationService = new PlaylistTrackMutationService(
            playlistRepository,
            playlistTrackRepository,
            trackSearchResolverService,
            trackSerializer,
            trackLoader,
            logger);
    }

    public async Task<SavePlaylistResult> SavePlaylistAsync(ulong guildId, string playlistName, string playlistUrl)
    {
        return await _trackMutationService.SavePlaylistAsync(guildId, playlistName, playlistUrl);
    }

    public async Task<PlaylistDto?> LoadPlaylistAsync(ulong guildId, string playlistName)
    {
        return await _queryService.LoadPlaylistAsync(guildId, playlistName);
    }

    public async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId)
    {
        return await _queryService.ListPlaylistsAsync(guildId);
    }

    public async Task<ViewPlaylistResult> ViewPlaylistAsync(ulong guildId, string playlistName)
    {
        return await _queryService.ViewPlaylistAsync(guildId, playlistName);
    }

    public async Task<DeletePlaylistResult> DeletePlaylistAsync(ulong guildId, string playlistName)
    {
        return await _mutationService.DeletePlaylistAsync(guildId, playlistName);
    }

    public async Task<AddSongResult> AddSongToPlaylistAsync(ulong guildId, string playlistName, string songUrl)
    {
        return await _trackMutationService.AddSongToPlaylistAsync(guildId, playlistName, songUrl);
    }

    public async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(ulong guildId, string playlistName, int trackNumber)
    {
        return await _trackMutationService.RemoveSongFromPlaylistAsync(guildId, playlistName, trackNumber);
    }

    public async Task<RenamePlaylistResult> RenamePlaylistAsync(ulong guildId, string currentName, string newName)
    {
        return await _mutationService.RenamePlaylistAsync(guildId, currentName, newName);
    }

    public async Task<CreatePlaylistResult> CreatePlaylistAsync(ulong guildId, string playlistName)
    {
        return await _mutationService.CreatePlaylistAsync(guildId, playlistName);
    }
}
