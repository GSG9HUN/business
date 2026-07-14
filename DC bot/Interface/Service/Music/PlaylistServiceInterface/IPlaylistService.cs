using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface;

public interface IPlaylistService
{
    Task<SavePlaylistResult> SavePlaylistAsync(ulong guildId, string playlistName, string playlistUrl);
    Task<PlaylistDto?> LoadPlaylistAsync(ulong guildId, string playlistName);
    Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId);
    Task<ViewPlaylistResult> ViewPlaylistAsync(ulong guildId, string playlistName);
    Task<DeletePlaylistResult> DeletePlaylistAsync(ulong guildId, string playlistName);
    Task<AddSongResult> AddSongToPlaylistAsync(ulong guildId, string playlistName, string songUrl);
    Task<RemoveSongResult> RemoveSongFromPlaylistAsync(ulong guildId, string playlistName, int trackNumber);
    Task<RenamePlaylistResult> RenamePlaylistAsync(ulong guildId, string currentName, string newName);
    Task<CreatePlaylistResult> CreatePlaylistAsync(ulong guildId, string playlistName);
}
