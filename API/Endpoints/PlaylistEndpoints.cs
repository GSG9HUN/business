using API.Handlers.Playlists;
using API.Validation;

namespace API.Endpoints;

public static class PlaylistEndpoints
{
    public static RouteGroupBuilder MapPlaylistEndpoints(this RouteGroupBuilder group)
    {
        var guildPlaylists = group
            .MapGroup("/guilds/{guildId}/playlists")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();

        guildPlaylists.MapGet("", PlaylistHandlers.ListAsync);
        guildPlaylists.MapPost("", PlaylistHandlers.CreateAsync);
        guildPlaylists.MapPost("/import", PlaylistHandlers.ImportAsync);

        var playlists = group
            .MapGroup("/playlists")
            .RequireAuthorization();

        playlists.MapGet("/{playlistId:long}", PlaylistHandlers.GetAsync);
        playlists.MapPatch("/{playlistId:long}/rename", PlaylistHandlers.RenameAsync);
        playlists.MapDelete("/{playlistId:long}", PlaylistHandlers.DeleteAsync);
        playlists.MapPost("/{playlistId:long}/load", PlaylistHandlers.LoadAsync);
        playlists.MapPost("/{playlistId:long}/tracks", PlaylistHandlers.AddTrackAsync);
        playlists.MapDelete("/{playlistId:long}/tracks/{trackNumber:int}", PlaylistHandlers.RemoveTrackAsync);

        return group;
    }
}
