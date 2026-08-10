using API.Handlers.Playback;
using API.Validation;

namespace API.Endpoints;

public static class PlaybackEndpoints
{
    public static RouteGroupBuilder MapPlaybackEndpoints(this RouteGroupBuilder group)
    {
        var playback = group
            .MapGroup("/guilds/{guildId}/playback")
            .RequireAuthorization()
            .AddEndpointFilter<GuildIdValidationFilter>();
        
        playback.MapPost("/{commandName}", PlaybackHandlers.ExecuteAsync)
            .AddEndpointFilter<CommandNameValidationFilter>();
        
        return group;
    }
}
