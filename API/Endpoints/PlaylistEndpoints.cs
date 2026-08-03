namespace API.Endpoints;

public static class PlaylistEndpoints
{
    public static RouteGroupBuilder MapPlaylistEndpoints(this RouteGroupBuilder group)
    {
       /* group.MapGet("guilds/{guildId}/playlists",
            async (ulong guildId, IPlaylistFacade facade, CancellationToken ct) => await facade.ListPlaylistsAsync<PlaylistDetail>(guildId, ct));
        
        
        //Ez még kérdéses
        /*group.MapPost("guilds/{guildId}/playlists/{playlistName}/load", 
            async (ulong guildId, IPlaylistFacade facade, CancellationToken ct) => await facade.SavePlaylistAsync<PlaylistDetail>(guildId, playlistName,ct));*/
      /*  group.MapGet("guilds/{guildId}/playlists/{playlistName}", 
            async (ulong guildId, string playlistName, IPlaylistFacade facade, CancellationToken ct) => await facade.ViewPlaylistAsync<PlaylistDetail>(guildId, playlistName, ct));
        group.MapPatch("guilds/{guildId}/playlists/{playlistName}/rename",
            async (ulong guildId, string playlistName, string newName, IPlaylistFacade facade, CancellationToken ct) => await facade.RenamePlaylistAsync<PlaylistDetail>(guildId, playlistName, newName, ct));
        group.MapDelete("guilds/{guildId}/playlists/{playlistName}", 
            async (ulong guildId, string playlistName, IPlaylistFacade facade, CancellationToken ct) => await facade.DeletePlaylistAsync<PlaylistDetail>(guildId, playlistName, ct));
        group.MapPost("guilds/{guildId}/playlists/{playlistName}/tracks",
            async (ulong guildId, string playlistName, string songUrl, IPlaylistFacade facade, CancellationToken ct) => await facade.AddSongToPlaylistAsync<PlaylistDetail>(guildId, playlistName, songUrl, ct));
        group.MapDelete("guilds/{guildId}/playlists/{playlistName}/tracks/{trackNumber}", 
            async (ulong guildId, string playlistName, int trackNumber, IPlaylistFacade facade, CancellationToken ct) => await facade.RemoveSongFromPlaylistAsync<PlaylistDetail>(guildId, playlistName, trackNumber, ct));
        group.MapPost("guilds/{guildId}/playlists/{playlistName}", 
            async (ulong guildId, string playlistName, IPlaylistFacade facade, CancellationToken ct) => await facade.CreatePlaylistAsync<PlaylistDetail>(guildId, playlistName, ct));
        group.MapPost("guilds/{guildId}/playlists/{playlistName}/load", 
            async (ulong guildId, string playlistName, IPlaylistFacade facade, CancellationToken ct) => await facade.LoadPlaylistAsync<PlaylistDetail>(guildId, playlistName, ct));
        */
        return group;
    }
}